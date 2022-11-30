using Common.Models.Data;
using Common.Models.DTO;

namespace Persistence.Repositories
{
    public interface IRepository<TDTO> where TDTO : class, IDTO
    {
        Task<TDTO> InsertAsync(TDTO dto);
        Task<TDTO?> FindOneAsync(TDTO dtoLike);
        Task<IEnumerable<TDTO>> FindAllAsync(PageToken<TDTO> pageToken);
    }
}