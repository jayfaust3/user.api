
using Common.Models.Data;
using Common.Models.DTO;
using Common.Utilities;
using Common.Exceptions;
using Persistence.Repositories;
using Application.Services.Cache;

namespace Application.Services.Crud;

public abstract class BaseCrudService<TDTO> : ICrudService<TDTO> where TDTO : class, IDTO
{
    protected readonly IRepository<TDTO> _repository;
    protected readonly ICacheService _cacheService;
    protected readonly ILogger _logger;

    public BaseCrudService
    (
        IRepository<TDTO> repository,
        ICacheService cacheService,
        ILogger logger
    )
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public virtual async Task<TDTO?> GetByIdAsync(Guid id)
    {
        var cacheKey = GetSingleEntryCacheKey(id);

        TDTO? match = await _cacheService.GetItemAsync<TDTO>(cacheKey);

        if (match == null)
        {
            _logger.LogInformation($"Cache miss for entry with key: '{cacheKey}'");

            match = await _repository.FindOneAsync(id);

            if (match != null)
            {
                try
                {
                    await _cacheService.SetItemAsync(cacheKey, match);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Unable to write entry with key: '{cacheKey}' to cache from single get, error: {ex.Message}");
                }
            }
        }
        else
        {
            _logger.LogInformation($"Cache hit for entry with key: '{cacheKey}'");
        }

        return match;
    }

    public virtual async Task<IEnumerable<TDTO>> GetAllAsync(string pageToken)
    {
        var pageCacheKey = GetPageCacheKey(pageToken);

        var matches = await _cacheService.GetItemAsync<IEnumerable<TDTO>>(pageCacheKey);

        if (matches == null)
        {
            _logger.LogInformation($"Cache miss for page with key: '{pageCacheKey}'");

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

            try
            {
                await _cacheService.SetItemAsync(pageToken, matches);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Unable to write page with key: '{pageCacheKey}' to cache, error: {ex.Message}");
            }

            var cacheSetTasks = new List<Task>();

            foreach (var match in matches)
            {
                var cacheKey = GetSingleEntryCacheKey(match.Id.Value);

                var operation = async () =>
                {
                    try
                    {
                        await _cacheService.SetItemAsync(cacheKey, match);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Unable to write entry with key: '{cacheKey}' to cache from batch get, error: {ex.Message}");
                    }
                };

                cacheSetTasks.Add(operation());
            }

            await Task.WhenAll(cacheSetTasks);
        }
        else
        {
            _logger.LogInformation($"Cache hit for page with key: '{pageCacheKey}'");
        }

        return matches;
    }

    public virtual async Task<TDTO> CreateAsync(TDTO recordToCreate)
    {
        return await _repository.InsertAsync(recordToCreate);
    }

    public virtual async Task<TDTO> UpdateAsync(TDTO recordToUpdate)
    {
        var cacheKey = GetSingleEntryCacheKey(recordToUpdate.Id.Value);

        try
        {
            await _cacheService.RemoveItemAsync<TDTO>(cacheKey);
        }
        catch(Exception ex)
        {
            _logger.LogError($"Unable to remove entry with key '{cacheKey}' from cache, error: '{ex.Message}'");
        }

        return await _repository.UpdateAsync(recordToUpdate);
    }

    protected abstract string GetSingleEntryCacheKey(Guid id);

    protected abstract string GetPageCacheKey(string pageToken);
}
