using Common.Models.DTO;
using Common.Models.Data;
using Common.Exceptions;
using Persistence.Repositories;
using Application.Services.Cache;

namespace Application.Services.Crud;

public class UserCrudService : BaseCrudService<UserDTO>, IUserCrudService
{

    public UserCrudService
    (
        IUserRepository userRepository,
        ICacheService cacheService,
        ILogger logger
    ) :
    base
    (
        userRepository,
        cacheService,
        logger
    ) {}

    public async Task<UserDTO?> GetByEmailAsync(string email)
    {
        UserDTO? match;

        var cacheKey = GetEmailCacheKey(email);

        match = await _cacheService.GetItemAsync<UserDTO>(cacheKey);

        if (match is null)
        {
            _logger.LogInformation($"Cache miss for entry with key: '{cacheKey}'");

            var pageToken = new PageToken
            {
                Cursor = 0,
                Limit = 1,
                Term = email
            };

            IEnumerable<UserDTO> matches = await _repository.FindAllAsync(pageToken);

            if (matches.Count() > 0)
            {
                match = matches.First();

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

    public override async Task<UserDTO> CreateAsync(UserDTO recordToCreate)
    {
        await ValidateIncomingRecord(recordToCreate);

        return await base.CreateAsync(recordToCreate);
    }

    public override async Task<UserDTO> UpdateAsync(UserDTO recordToUpdate)
    {
        await ValidateIncomingRecord(recordToUpdate);

        return await base.UpdateAsync(recordToUpdate);
    }

    protected override string GetSingleEntryCacheKey(Guid recordId) => $"USER_ENTRY:{recordId}";

    private async Task ValidateIncomingRecord(UserDTO recordToWrite)
    {
        var email = recordToWrite.EmailAddress;

        UserDTO? potentialExistingUserWithMatchingEmail = await GetByEmailAsync(email);

        if (potentialExistingUserWithMatchingEmail != null) throw new ConflictException($"User with email: '{email}' already exists");
    }

    private static string GetEmailCacheKey(string email) => $"USER_ENTRY_EMAIL:{email}";
}
