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
            .DefaultIndex(settings.IndexName);

        return new OpenSearchClient(connectionSettings);
    }

    public async Task<bool> InsertAsync(TEntity entity)
    {
        entity.Id = Guid.NewGuid();

        var request = new IndexRequest<TEntity>(entity);

        IndexResponse response = await _client.IndexAsync(request);

        return response.IsValid;
    }

    public async Task<IEnumerable<TEntity>> FindAllAsync(TEntity entityLike)
    {
        var request = GenerateSearchRequest(entityLike);

        ISearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        return response.Documents;

    }

    protected abstract ISearchRequest GenerateSearchRequest(TEntity entityLike);
}

