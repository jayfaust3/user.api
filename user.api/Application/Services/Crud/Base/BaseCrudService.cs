
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

        if (match is null)
        {
            _logger.LogInformation($"Cache miss for entry with key: '{cacheKey}'");

            match = await _repository.FindOneAsync(id);

            if (match is not null)
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
        PageToken parsedToken;

        try
        {
            parsedToken = PagingUtilities.ParsePageToken(pageToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Unable to parse page token, error: '{ex.Message}'");
        }

        IEnumerable<TDTO> matches = await _repository.FindAllAsync(parsedToken);

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
            _logger.LogError($"Unable to remove entry with key '{cacheKey}' from cache on update, error: '{ex.Message}'");
        }

        return await _repository.UpdateAsync(recordToUpdate);
    }

    public virtual async Task DeleteByIdAsync(Guid id)
    {
        TDTO? match = await GetByIdAsync(id);

        if (match is null) throw new NotFoundException($"Unable to delete entry, no entry found with id '{id}'");

        await _repository.DeleteAsync(id);

        var cacheKey = GetSingleEntryCacheKey(id);

        try
        {
            await _cacheService.RemoveItemAsync<TDTO>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unable to remove entry with key '{cacheKey}' from cache on delete, error: '{ex.Message}'");
        }
    }

    protected abstract string GetSingleEntryCacheKey(Guid id);
}
