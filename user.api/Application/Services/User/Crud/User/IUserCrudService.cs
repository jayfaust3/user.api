using Common.Models.Data;
using Common.Models.DTO;

namespace Application.Services
{
    public interface IUserCrudService
    {
        Task<UserDTO> CreateAsync(UserDTO recordToCreate);
        Task<IEnumerable<UserDTO>> GetAllAsync(PageToken pageToken);
        Task<UserDTO?> GetByEmailAsync(string email);
        Task<UserDTO?> GetByIdAsync(Guid id);
    }
}