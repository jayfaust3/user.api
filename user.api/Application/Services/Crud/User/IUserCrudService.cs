using Common.Models.DTO;

namespace Application.Services.Crud
{
    public interface IUserCrudService : ICrudService<UserDTO>
    {
        Task<UserDTO?> GetByEmailAsync(string email);
    }
}