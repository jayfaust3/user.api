using Common.Models.DTO;

namespace Persistence.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, IDTO
    {
        Task<IEnumerable<TEntity>> FindAllAsync(TEntity entityLike);
        Task<bool> InsertAsync(TEntity entity);
    }
}