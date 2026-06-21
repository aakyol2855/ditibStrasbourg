using System.Linq.Expressions;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Base
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        internal DbSet<TEntity> dbSet;
        private readonly ILogger<BaseService<TEntity>> _logger;

        public BaseService(ApplicationDbContext context, ILogger<BaseService<TEntity>> logger)
        {
            _context = context;
            this.dbSet = _context.Set<TEntity>();
            _logger = logger;
        }

        public virtual IQueryable<TEntity> GetQueryable(bool tracking = false)
        {
            return tracking ? dbSet : dbSet.AsNoTracking();
        }

        public virtual async Task<PaginatedList<TEntity>> GetPaginatedAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 20)
        {
            try
            {
                IQueryable<TEntity> query = dbSet.AsNoTracking();

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                return await PaginatedList<TEntity>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated list for {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            return await query.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            query = query.Where(filter);

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            try
            {
                await dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added new {EntityType}", typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            try
            {
                dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated {EntityType}", typeof(TEntity).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public virtual async Task DeleteAsync(object id)
        {
            TEntity? entityToDelete = await dbSet.FindAsync(id);
            if (entityToDelete != null)
            {
                await DeleteAsync(entityToDelete);
            }
        }

        public virtual async Task DeleteAsync(TEntity entityToDelete)
        {
            try
            {
                if (_context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }

                var isDeletedProp = typeof(TEntity).GetProperty("IsDeleted");
                if (isDeletedProp != null && isDeletedProp.PropertyType == typeof(bool) && isDeletedProp.CanWrite)
                {
                    isDeletedProp.SetValue(entityToDelete, true);
                    _context.Entry(entityToDelete).State = EntityState.Modified;
                    _logger.LogInformation("Soft-deleted {EntityType}", typeof(TEntity).Name);
                }
                else
                {
                    dbSet.Remove(entityToDelete);
                    _logger.LogInformation("Hard-deleted {EntityType} (No IsDeleted property)", typeof(TEntity).Name);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }
    }
}
