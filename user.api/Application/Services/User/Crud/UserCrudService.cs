using Persistence.Repositories;
using Common.Models.DTO;
using Common.Models.Data;

namespace Application.Services;

public class UserCrudService : IUserCrudService
{
    private readonly IUserRepository _userRepository;

    public UserCrudService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDTO?> GetByIdAsync(Guid id)
    {
        return await _userRepository.FindOneAsync(id);
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
        return await _userRepository.InsertAsync(recordToCreate);
    }
}
