using Persistence.Repositories;
using Common.Models.DTO;
using Common.Models.Data;
using user.api.Application.Services.Cache;

namespace Application.Services;

public class UserCrudService : IUserCrudService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;

    public UserCrudService(IUserRepository userRepository, ICacheService cacheService)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    public async Task<UserDTO?> GetByIdAsync(Guid id)
    {
        var cacheKey = GetCacheKey(id);

        UserDTO? match = await _cacheService.GetItemAsync<UserDTO>(cacheKey);

        if (match == null)
        {
            match = await _userRepository.FindOneAsync(id);

            await _cacheService.SetItemAsync(cacheKey, match);
        }

        return match;
    }

    public async Task<UserDTO?> GetByEmailAsync(string email)
    {
        var pageToken = new PageToken
        {
            Cursor = 0,
            Limit = 1,
            Term = email
        };

        IEnumerable<UserDTO> matches = await _userRepository.FindAllAsync(pageToken);

        return matches.First();
    }

    public async Task<IEnumerable<UserDTO>> GetAllAsync(PageToken pageToken)
    {
        return await _userRepository.FindAllAsync(pageToken);
    }

    public async Task<UserDTO> CreateAsync(UserDTO recordToCreate)
    {
        var createdRecord = await _userRepository.InsertAsync(recordToCreate);

        var cacheKey = GetCacheKey(createdRecord.Id.Value);

        await _cacheService.SetItemAsync(cacheKey, createdRecord);

        return createdRecord;
    }

    private static string GetCacheKey(Guid recordId)
    {
        return $"USER:{recordId}";
    }
}
