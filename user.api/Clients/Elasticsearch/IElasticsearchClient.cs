using Common.Models.Data;
using Elastic.Clients.Elasticsearch;

namespace Clients.Elasticsearch;

public interface IElasticsearchClient
{
    string IndexName { get; }
    Task<GetResponse<TEntity>> GetAsync<TEntity>(GetRequest request) where TEntity : class, IEntity;
    Task<SearchResponse<TEntity>> SearchAsync<TEntity>(SearchRequest<TEntity> request) where TEntity : class, IEntity;
    Task<IndexResponse> IndexAsync<TEntity>(IndexRequest<TEntity> request) where TEntity : class, IEntity;
}
