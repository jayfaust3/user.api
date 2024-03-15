using Clients.Elasticsearch;
using Common.Models.Context;
using Common.Models.Data;
using Common.Models.DTO;
using Utilities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace Persistence.Repositories;

public abstract class BaseRepository<TEntity, TDTO> : IRepository<TDTO>
    where TEntity : class, IEntity
    where TDTO : class, IDTO
{
    private readonly IElasticsearchClient _client;

    private readonly IUserContext _userContext;

    protected readonly IEnumerable<string> _defaultFieldsExcludedFromSearch = typeof(IEntity)
        .GetProperties()
        .Select(p => p.Name);

    private readonly HashSet<string> _fieldsExcludedFromSearch;

    protected BaseRepository
    (
        IElasticsearchClient client,
        IUserContext userContext,
        IEnumerable<string>? fieldsExcludedFromSearch = null
    )
    {
        _client = client;
        _userContext = userContext;
        _fieldsExcludedFromSearch = new HashSet<string>
        (
            fieldsExcludedFromSearch ?? _defaultFieldsExcludedFromSearch
        );
    }

    private IEnumerable<string> GetAllSearchableFields() =>
        typeof(TEntity)
        .GetProperties()
        .Select(p => p.Name)
        .Where(field => !_fieldsExcludedFromSearch.Contains(field));

    protected virtual GetRequest GenerateFindOneGetRequest(Guid id) => new GetRequest(_client.IndexName, id);

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

        var timestamp = TimeUtilities.GetUnixEpoch();
        var userId = _userContext.Id;

        entity.id = Guid.NewGuid();
        entity.created_on = timestamp;
        entity.updated_on = timestamp;
        entity.created_by = userId;
        entity.updated_by = userId;

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

        SearchResponse<TEntity> response = await _client.SearchAsync(request);

        if (response.IsSuccess())
            results = response.Documents.Select(MapToDTO);
        else
            throw response.ApiCallDetails.OriginalException;

        return results;
    }
}
