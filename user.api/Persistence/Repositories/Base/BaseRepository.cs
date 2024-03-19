using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Clients.Elasticsearch;
using Common.Models.Context;
using Common.Models.Data;
using Common.Models.DTO;
using Common.Utilities;
using Common.Exceptions;
using System.Linq;
using System.Collections.ObjectModel;

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
        Query? query = null;

        var entityFieldNames = GetAllSearchableFields();

        if (!string.IsNullOrWhiteSpace(pageToken.Term) && entityFieldNames != null)
        {
            
            Fields? fields = null;

            foreach (var fieldName in entityFieldNames)
            {
                if ((fields?.Count() ?? 0) == 0)
                    fields = new Field(fieldName);
                else
                    fields = fields?.And(new Field(fieldName));

            }

            query = new MultiMatchQuery
            {
                Query = pageToken.Term,
                Type = TextQueryType.Phrase,
                Fields = fields
            };
        }


        return new SearchRequest<TEntity>
        {
            From = pageToken.Cursor,
            Size = pageToken.Limit,
            Query = query
        };
    }

    protected abstract TEntity MapToEntity(TDTO dto);

    protected abstract TDTO MapToDTO(TEntity dto);

    public async Task<TDTO?> FindOneAsync(Guid id)
    {
        TEntity? match = await FindEntityAsync(id);            

        return match != null ? MapToDTO(match) : null;
    }

    private async Task<TEntity?> FindEntityAsync(Guid id)
    {
        GetRequest request = GenerateFindOneGetRequest(id);

        GetResponse<TEntity> response = await _client.GetAsync<TEntity>(request);

        if (!response.IsSuccess())
            throw response.ApiCallDetails.OriginalException;

        return response.Source;
    }

    public async Task<IEnumerable<TDTO>> FindAllAsync(PageToken pageToken)
    {
        SearchRequest<TEntity> request = GenerateFindAllSearchRequest(pageToken);

        SearchResponse<TEntity> response = await _client.SearchAsync(request);

        if (!response.IsSuccess())
            throw response.ApiCallDetails.OriginalException;

        IReadOnlyCollection<TEntity> matches = response.Documents;

        return matches.Count > 0 ? matches.Select(MapToDTO) : new List<TDTO>();            
    }

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

        if (!response.IsSuccess())
            throw response.ApiCallDetails.OriginalException;

        return MapToDTO(entity);
    }

    public async Task<TDTO> UpdateAsync(TDTO dto)
    {
        var entityId = dto.Id.Value;

        TEntity? currentStateEntity = await FindEntityAsync(entityId);

        if (currentStateEntity == null) throw new NotFoundException($"No entity with id '{entityId}' found");

        TEntity entity = MapToEntity(dto);

        var timestamp = TimeUtilities.GetUnixEpoch();
        var userId = _userContext.Id;

        entity.id = entityId;
        entity.created_by = currentStateEntity.created_by;
        entity.created_on = currentStateEntity.created_on;
        entity.updated_on = timestamp;
        entity.updated_by = userId;

        var updateRequest = new UpdateRequest<TEntity, TEntity>(_client.IndexName, entity.id)
        {
            Doc = entity,
        };

        UpdateResponse<TEntity> response = await _client.UpdateAsync(updateRequest);

        if (!response.IsSuccess())
            throw response.ApiCallDetails.OriginalException;

        return MapToDTO(entity);
    }
}
