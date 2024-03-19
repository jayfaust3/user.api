﻿
using Common.Models.Data;
using Common.Models.DTO;
using Common.Utilities;
using Persistence.Repositories;
using Application.Services.Cache;
using Common.Exceptions;

namespace Application.Services.Crud;

public abstract class BaseCrudService<TDTO> : ICrudService<TDTO> where TDTO : class, IDTO
{
    protected readonly IRepository<TDTO> _repository;
    protected readonly ICacheService _cacheService;

    public BaseCrudService
    (
        IRepository<TDTO> repository,
        ICacheService cacheService
    )
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public virtual async Task<TDTO?> GetByIdAsync(Guid id)
    {
        var cacheKey = GetSingleEntryCacheKey(id);

        TDTO? match = await _cacheService.GetItemAsync<TDTO>(cacheKey);

        if (match == null)
        {
            match = await _repository.FindOneAsync(id);

            await _cacheService.SetItemAsync(cacheKey, match);
        }

        return match;
    }

    public virtual async Task<IEnumerable<TDTO>> GetAllAsync(string pageToken)
    {
        var pageCacheKey = GetPageCacheKey(pageToken);

        var matches = await _cacheService.GetItemAsync<IEnumerable<TDTO>>(pageCacheKey);

        if (matches == null)
        {
            PageToken parsedToken = null;

            try
            {
                parsedToken = PagingUtilities.ParsePageToken(pageToken);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Unable to parse page token, error: '{ex.Message}'");
            }
            
            matches = await _repository.FindAllAsync(parsedToken);

            await _cacheService.SetItemAsync(pageToken, matches);

            var cacheSetTasks = new List<Task>();

            foreach (var match in matches)
            {
                var cacheKey = GetSingleEntryCacheKey(match.Id.Value);

                cacheSetTasks.Add(_cacheService.SetItemAsync(cacheKey, match));
            }

            await Task.WhenAll(cacheSetTasks);
        }

        return matches;
    }

    public virtual async Task<TDTO> CreateAsync(TDTO recordToCreate)
    {
        var cacheKey = GetSingleEntryCacheKey(recordToCreate.Id.Value);

        await _cacheService.RemoveItemAsync<TDTO>(cacheKey);

        return await _repository.InsertAsync(recordToCreate);
    }

    public virtual async Task<TDTO> UpdateAsync(TDTO recordToUpdate)
    {
        var cacheKey = GetSingleEntryCacheKey(recordToUpdate.Id.Value);

        await _cacheService.RemoveItemAsync<TDTO>(cacheKey);

        return await _repository.UpdateAsync(recordToUpdate);
    }

    protected abstract string GetSingleEntryCacheKey(Guid id);

    protected abstract string GetPageCacheKey(string pageToken);
}
