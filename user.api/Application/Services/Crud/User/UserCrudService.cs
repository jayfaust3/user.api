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
        ICacheService cacheService
    ) :
    base
    (
        userRepository,
        cacheService
    ) {}

    public async Task<UserDTO?> GetByEmailAsync(string email)
    {
        var pageToken = new PageToken
        {
            Cursor = 0,
            Limit = 1,
            Term = email
        };

        IEnumerable<UserDTO> matches = await _repository.FindAllAsync(pageToken);

        return matches.First();
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

    protected override string GetPageCacheKey(string pageToken) => $"USER_PAGE:{pageToken}";

    private async Task ValidateIncomingRecord(UserDTO recordToWrite)
    {
        var email = recordToWrite.EmailAddress;

        UserDTO? potentialExistingUserWithMatchingEmail = await GetByEmailAsync(email);

        if (potentialExistingUserWithMatchingEmail != null) throw new ConflictException($"User with email: '{email}' already exists");
    }
}
