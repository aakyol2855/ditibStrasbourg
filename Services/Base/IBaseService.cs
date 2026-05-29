using System.Linq.Expressions;
using DitibStasbourg.Models;

namespace DitibStasbourg.Services.Base
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetQueryable(bool tracking = false);
        Task<PaginatedList<TEntity>> GetPaginatedAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 20);

        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "");

        Task<TEntity?> GetByIdAsync(object id);

        Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter,
            string includeProperties = "");

        Task AddAsync(TEntity entity);
        
        Task UpdateAsync(TEntity entity);
        
        Task DeleteAsync(object id);
        
        Task DeleteAsync(TEntity entity);
    }
}
