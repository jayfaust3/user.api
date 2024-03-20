using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticsearchPackageClient = Elastic.Clients.Elasticsearch.ElasticsearchClient;
using Common.Models.Configuration;
using Common.Models.Data;

namespace Clients.Elasticsearch;

public class ElasticsearchClient : IElasticsearchClient
{
    public string IndexName { get; }

    private readonly ElasticsearchPackageClient _client;

    public ElasticsearchClient(IElasticsSearchSettings settings)
    {
        IndexName = settings.IndexName;
        _client = GenerateClient(settings);
    }

    public async Task<GetResponse<TEntity>> GetAsync<TEntity>(GetRequest request) where TEntity : class, IEntity
    {
        return await _client.GetAsync<TEntity>(request);
    }

    public async Task<SearchResponse<TEntity>> SearchAsync<TEntity>(SearchRequest<TEntity> request) where TEntity : class, IEntity
    {
        return await _client.SearchAsync<TEntity>(request);
    }

    public async Task<IndexResponse> IndexAsync<TEntity>(IndexRequest<TEntity> request) where TEntity : class, IEntity
    {
        return await _client.IndexAsync(request);
    }

    public async Task<UpdateResponse<TEntity>> UpdateAsync<TEntity>(UpdateRequest<TEntity, TEntity> request) where TEntity : class, IEntity
    {
        return await _client.UpdateAsync(request);
    }

    public async Task<DeleteResponse> DeleteAsync(DeleteRequest request)
    {
        return await _client.DeleteAsync(request);
    }

    private ElasticsearchPackageClient GenerateClient(IElasticsSearchSettings settings)
    {
        var nodeURIs = settings.NodeURIs.Select(uri => new Uri(uri));

        var pool = new StaticNodePool(nodeURIs);

        var connectionSettings = new ElasticsearchClientSettings(pool)
            .DefaultMappingFor<IEntity>
            (
                m =>
                {
                    m.IndexName(IndexName);
                    m = m.IdProperty("id");
                }
            ).DefaultIndex(IndexName);

        return new ElasticsearchPackageClient(connectionSettings);
    }
}
