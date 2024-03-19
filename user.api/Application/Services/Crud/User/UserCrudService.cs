using Persistence.Repositories;
using Common.Models.DTO;
using Common.Models.Data;
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

    protected override string GetCacheKey(Guid recordId) => $"USER:{recordId}";
}
