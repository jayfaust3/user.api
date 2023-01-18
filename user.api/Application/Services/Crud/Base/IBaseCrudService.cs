using Common.Models.Data;
using Common.Models.DTO;

namespace Application.Services.Crud
{
    public interface ICrudService<TDTO> where TDTO : class, IDTO
    {
        Task<TDTO> CreateAsync(TDTO recordToCreate);
        Task<IEnumerable<TDTO>> GetAllAsync(PageToken pageToken);
        Task<TDTO?> GetByIdAsync(Guid id);
    }
}