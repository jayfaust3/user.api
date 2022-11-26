using Common.Models.Configuration;
using Common.Models.DTO;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Persistence.Repositories;

public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class, IDTO
{
    IOpenSearchClient _client;

    protected BaseRepository(IOpenSearchSettings settings) => _client = GenerateClient(settings);

    private static IOpenSearchClient GenerateClient(IOpenSearchSettings settings)
    {
        var pool = new StaticConnectionPool
        (
            settings.NodeURIs.Select(uri => new Uri(uri))
        );

        var connectionSettings = new ConnectionSettings(pool)
            .DefaultMappingFor<TEntity>(
                m =>
                {
                    m.IndexName(settings.IndexName);
                    m = m.IdProperty("Id");
                    return m;
                }
            );

        return new OpenSearchClient(connectionSettings);
    }

    private static long GetUnixEpoch() =>
        (
            (long)
            (
                DateTime.Now.ToUniversalTime() -
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds
        );

    protected ISearchRequest GenerateFindOneSearchRequest(TEntity entityLike)
    {
        return new SearchRequest<UserDTO>
        {
            From = 0,
            Size = 1,
            Query = new MatchQuery
            {
                Field = Infer.Field<UserDTO>(f => f.Id),
                Query = entityLike.Id?.ToString()
            }
        };
    }

    protected abstract ISearchRequest GenerateFindAllSearchRequest(TEntity entityLike);

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedOn = GetUnixEpoch();

        var request = new IndexRequest<TEntity>(entity);

        IndexResponse response = await _client.IndexAsync(request);

        if (!response.IsValid) throw new Exception("Unable to insert record");

        return entity;
    }

    public async Task<TEntity?> FindOneAsync(TEntity entityLike)
    {
        var request = GenerateFindAllSearchRequest(entityLike);

        ISearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        return response.Documents.FirstOrDefault();

    }

    public async Task<IEnumerable<TEntity>> FindAllAsync(TEntity entityLike)
    {
        var request = GenerateFindAllSearchRequest(entityLike);

        ISearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        return response.Documents;

    }
}

