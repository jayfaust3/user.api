using Common.Models.Configuration;
using Common.Models.Data;
using Common.Models.DTO;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

namespace Persistence.Repositories;

public abstract class BaseRepository<TEntity, TDTO> : IRepository<TDTO> where TEntity : class, IEntity where TDTO : class, IDTO
{
    private ElasticsearchClient _client;

    protected BaseRepository(IOpenSearchSettings settings) => _client = GenerateClient(settings);

    private static ElasticsearchClient GenerateClient(IOpenSearchSettings settings)
    {
        var pool = new StaticNodePool
        (
            settings.NodeURIs.Select(uri => new Uri(uri))
        );

        var connectionSettings = new ElasticsearchClientSettings(pool)
            .DefaultMappingFor<TEntity>
            (
                m =>
                {
                    m.IndexName(settings.IndexName);
                    m = m.IdProperty("id");
                }
            ).DefaultIndex(settings.IndexName);

        return new ElasticsearchClient(connectionSettings);
    }

    private static int GetUnixEpoch() =>
        (
            (int)
            (
                DateTime.Now.ToUniversalTime() -
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds
        );

    private IEnumerable<string> GetAllSearchableFields() =>
        typeof(TEntity)
        .GetProperties()
        .Select(p => p.Name)
        .Where(n => n != "id" && n != "created_on");

    protected SearchRequest GenerateFindOneSearchRequest(Guid id)
    {
        return new SearchRequest<UserDTO>
        {
            From = 0,
            Size = 1,
            Query = new TermQuery
            (Infer.Field<UserDTO>(f => f.Id))
            {
                Value = id.ToString()
            }
        };
    }

    protected abstract TEntity MapToEntity(TDTO dto);

    protected abstract TDTO MapToDTO(TEntity dto);

    public async Task<TDTO> InsertAsync(TDTO dto)
    {
        var entity = MapToEntity(dto);

        entity.id = Guid.NewGuid();
        entity.created_on = GetUnixEpoch();

        var request = new IndexRequest<TEntity>(entity);

        IndexResponse response = await _client.IndexAsync(request);

        if (!response.IsSuccess()) throw new Exception("Unable to insert record");

        return MapToDTO(entity);
    }

    public async Task<TDTO?> FindOneAsync(Guid id)
    {
        var request = GenerateFindOneSearchRequest(id);

        SearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        var match = response.Documents.FirstOrDefault();

        return match != null ? MapToDTO(match) : null;
    }

    public async Task<IEnumerable<TDTO>> FindAllAsync(PageToken pageToken)
    {
        IEnumerable<TDTO> results = new List<TDTO>();

        SearchResponse<TEntity> response = await _client.SearchAsync<TEntity>
            (
                s =>
                    s
                    .Query
                    (
                        q =>
                            q
                            .Bool
                            (
                                b =>
                                    b
                                    .Should
                                    (
                                        bs =>
                                            GetAllSearchableFields().Select<string, Query>
                                            (
                                                fieldName =>
                                                    new TermQuery
                                                    (
                                                        new Field(fieldName)
                                                    )
                                                    {
                                                        Value = pageToken.Term
                                                    }
                                            )
                                    )
                            )
                    )
            );

        if (response.IsSuccess())
            results = response.Documents.Select(MapToDTO);
        else
            throw response.ApiCallDetails.OriginalException;

        return results;

    }
}

