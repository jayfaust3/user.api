using Common.Models.Data;
using Common.Models.DTO;

namespace Persistence.Repositories
{
    public interface IRepository<TDTO> where TDTO : class, IDTO
    {
        Task<TDTO> InsertAsync(TDTO dto);
        Task<TDTO?> FindOneAsync(Guid id);
        Task<IEnumerable<TDTO>> FindAllAsync(PageToken pageToken);
        Task<TDTO> UpdateAsync(TDTO dto);
        Task DeleteAsync(Guid id);
    }
}