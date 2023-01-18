using Common.Models.Configuration;
using Common.Models.Context;
using Common.Models.Data;
using Common.Models.DTO;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;

namespace Persistence.Repositories;

public abstract class BaseRepository<TEntity, TDTO> : IRepository<TDTO>
    where TEntity : class, IEntity
    where TDTO : class, IDTO
{
    private readonly string _indexName;
    private readonly ElasticsearchClient _client;
    private readonly IUserContext _userContext;

    protected BaseRepository(IOpenSearchSettings settings, IUserContext userContext)
    {
        _indexName = settings.IndexName;
        _client = GenerateClient(settings);
        _userContext = userContext;
    }

    private static ElasticsearchClient GenerateClient(IOpenSearchSettings settings)
    {
        var nodeURIs = settings.NodeURIs.Select(uri => new Uri(uri));

        var pool = new StaticNodePool(nodeURIs);

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
        .Where
        (
            name =>
                name != "id" &&
                name != "created_on"
        );

    protected virtual GetRequest GenerateFindOneGetRequest(Guid id) => new GetRequest(_indexName, id);

    protected virtual SearchRequest<TEntity> GenerateFindAllSearchRequest(PageToken pageToken)
    {
        var entityFields = GetAllSearchableFields();

        Fields? fields = null;

        foreach (var fieldName in entityFields)
        {
            if ((fields?.Count() ?? 0) == 0)
                fields = new Field(fieldName);
            else
                fields = fields?.And(new Field(fieldName));

        }

        return new SearchRequest<TEntity>
        {
            From = pageToken.Cursor,
            Size = pageToken.Limit,
            Query = new MultiMatchQuery
            {
                Query = pageToken.Term,
                Fields = fields
            }
        };
    }

    protected abstract TEntity MapToEntity(TDTO dto);

    protected abstract TDTO MapToDTO(TEntity dto);

    public async Task<TDTO> InsertAsync(TDTO dto)
    {
        TEntity entity = MapToEntity(dto);

        entity.id = Guid.NewGuid();
        entity.created_on = GetUnixEpoch();

        var request = new IndexRequest<TEntity>(entity);

        IndexResponse response = await _client.IndexAsync(request);

        if (!response.IsSuccess()) throw response.ApiCallDetails.OriginalException;

        return MapToDTO(entity);
    }

    public async Task<TDTO?> FindOneAsync(Guid id)
    {
        TEntity? match = null;

        GetRequest request = GenerateFindOneGetRequest(id);

        GetResponse<TEntity> response = await _client.GetAsync<TEntity>(request);

        if (response.IsSuccess())
            match = response.Source;
        else
            throw response.ApiCallDetails.OriginalException;

        return match != null ? MapToDTO(match) : null;
    }

    public async Task<IEnumerable<TDTO>> FindAllAsync(PageToken pageToken)
    {
        IEnumerable<TDTO> results = new List<TDTO>();

        SearchRequest<TEntity> request = GenerateFindAllSearchRequest(pageToken);

        SearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        if (response.IsSuccess())
            results = response.Documents.Select(MapToDTO);
        else
            throw response.ApiCallDetails.OriginalException;

        return results;
    }
}
