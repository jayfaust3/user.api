using Application.Services.Cache;
using Common.Models.Data;
using Common.Models.DTO;
using Persistence.Repositories;

namespace Application.Services.Crud;

public abstract class BaseCrudService<TDTO> : ICrudService<TDTO> where TDTO : class, IDTO
{
    protected readonly IRepository<TDTO> _repository;
    protected readonly ICacheService _cacheService;

    public BaseCrudService(IRepository<TDTO> repository, ICacheService cacheService)
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

    public async Task<IEnumerable<TDTO>> GetAllAsync(PageToken pageToken)
    {
        return await _repository.FindAllAsync(pageToken);
    }

    public async Task<TDTO> CreateAsync(TDTO recordToCreate)
    {
        var createdRecord = await _repository.InsertAsync(recordToCreate);

        var cacheKey = GetCacheKey(createdRecord.Id.Value);

        await _cacheService.SetItemAsync(cacheKey, createdRecord);

        return createdRecord;
    }

    protected abstract string GetCacheKey(Guid recordId);
}
