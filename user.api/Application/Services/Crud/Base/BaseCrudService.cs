using Application.Services.Cache;
using Common.Models.Data;
using Common.Models.DTO;
using Persistence.Repositories;
using Utilities;

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

    public async Task<TDTO?> GetByIdAsync(Guid id)
    {
        var cacheKey = GetCacheKey(id);

        TDTO? match = await _cacheService.GetItemAsync<TDTO>(cacheKey);

        if (match == null)
        {
            match = await _repository.FindOneAsync(id);

            await _cacheService.SetItemAsync(cacheKey, match);
        }

        return match;
    }

    public async Task<IEnumerable<TDTO>> GetAllAsync(string pageToken)
    {
        var matches = await _cacheService.GetItemAsync<IEnumerable<TDTO>>(pageToken);

        if (matches == null)
        {
            PageToken parsedToken = PagingUtilities.ParsePageToken(pageToken);

            matches = await _repository.FindAllAsync(parsedToken);

            await _cacheService.SetItemAsync(pageToken, matches);

            IEnumerable<Task> cacheSetTasks = new List<Task>();

            foreach (var match in matches)
            {
                var cacheKey = GetCacheKey(match.Id.Value);

                await _cacheService.SetItemAsync(cacheKey, match);
            }

            await Task.WhenAll(cacheSetTasks);
        }

        return matches;
    }

    public async Task<TDTO> CreateAsync(TDTO recordToCreate) =>
        await _repository.InsertAsync(recordToCreate);

    protected abstract string GetCacheKey(Guid id);
}
