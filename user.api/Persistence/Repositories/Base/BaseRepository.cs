﻿using Common.Models.Configuration;
using Common.Models.Data;
using Common.Models.DTO;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Persistence.Repositories;

public abstract class BaseRepository<TEntity, TDTO> : IRepository<TDTO> where TEntity : class, IEntity where TDTO : class, IDTO
{
    private IOpenSearchClient _client;

    protected BaseRepository(IOpenSearchSettings settings) => _client = GenerateClient(settings);

    private static IOpenSearchClient GenerateClient(IOpenSearchSettings settings)
    {
        var pool = new StaticConnectionPool
        (
            settings.NodeURIs.Select(uri => new Uri(uri))
        );

        var connectionSettings = new ConnectionSettings(pool)
            .DefaultMappingFor<TEntity>
            (
                m =>
                {
                    m.IndexName(settings.IndexName);
                    m = m.IdProperty("id");
                    return m;
                }
            ).DefaultIndex(settings.IndexName);

        return new OpenSearchClient(connectionSettings);
    }

    private static int GetUnixEpoch() =>
        (
            (int)
            (
                DateTime.Now.ToUniversalTime() -
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds
        );

    protected IEnumerable<string> GetAllSearchableFields() =>
        typeof(TEntity)
        .GetProperties()
        .Select(p => p.Name)
        .Where(n => n != "id" && n != "created_on");

    protected ISearchRequest GenerateFindOneSearchRequest(Guid id)
    {
        return new SearchRequest<UserDTO>
        {
            From = 0,
            Size = 1,
            Query = new MatchQuery
            {
                Field = Infer.Field<UserDTO>(f => f.Id),
                Query = id.ToString()
            }
        };
    }

    protected abstract ISearchRequest GenerateFindAllSearchRequest(PageToken pageToken);

    protected abstract TEntity MapToEntity(TDTO dto);

    protected abstract TDTO MapToDTO(TEntity dto);

    public async Task<TDTO> InsertAsync(TDTO dto)
    {
        var entity = MapToEntity(dto);

        entity.id = Guid.NewGuid();
        entity.created_on = GetUnixEpoch();

        var request = new IndexRequest<TEntity>(entity);

        IndexResponse response = await _client.IndexAsync(request);

        if (!response.IsValid) throw new Exception("Unable to insert record");

        return MapToDTO(entity);
    }

    public async Task<TDTO?> FindOneAsync(Guid id)
    {
        var request = GenerateFindOneSearchRequest(id);

        ISearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        var match = response.Documents.FirstOrDefault();

        return match != null ? MapToDTO(match) : null;
    }

    public async Task<IEnumerable<TDTO>> FindAllAsync(PageToken pageToken)
    {
        var request = GenerateFindAllSearchRequest(pageToken);

        ISearchResponse<TEntity> response = await _client.SearchAsync<TEntity>(request);

        return response.Documents.Select(MapToDTO);

    }
}

