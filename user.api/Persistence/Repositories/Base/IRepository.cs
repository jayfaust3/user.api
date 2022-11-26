using Common.Models.DTO;

namespace Persistence.Repositories
{
    public interface IRepository<TEntity> where TEntity : class, IDTO
    {
        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity?> FindOneAsync(TEntity entityLike);
        Task<IEnumerable<TEntity>> FindAllAsync(TEntity entityLike);
    }
}