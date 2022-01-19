using CommunityData.Data.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace .CommunityData.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ILogger Logger;
        protected readonly CancellationToken CancellationToken;
        protected readonly IMfhDbContext MfhDbContext;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(IMfhDbContext mfhDbContext, CancellationToken cancellationToken)
        {
            MfhDbContext = mfhDbContext;
            DbSet = MfhDbContext.Set<TEntity>();

            Logger = LogManager.GetCurrentClassLogger();
            CancellationToken = cancellationToken;
        }

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .FirstOrDefaultAsync(predicate, CancellationToken);
        }

        public Task<List<TEntity>> FindAllAsync(params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredAsync(Expression<Func<TEntity, bool>> predicate, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllOrderByAscAsync(Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderBy(keySelector)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredOrderByAscAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .OrderBy(keySelector)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllOrderByDescAsync(Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderByDescending(keySelector)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredOrderByDescAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .OrderByDescending(keySelector)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllTakeAsync(int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllAscTakeAsync(Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderBy(keySelector)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllDescTakeAsync(Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderByDescending(keySelector)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredTakeAsync(Expression<Func<TEntity, bool>> predicate, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredAscTakeAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .OrderBy(keySelector)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllFilteredDescTakeAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .Where(predicate)
                .OrderByDescending(keySelector)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllSkipTakeAscAsync(Expression<Func<TEntity, object>> keySelector, int skip, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderBy(keySelector)
                .Skip(skip)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public Task<List<TEntity>> FindAllSkipTakeDescAsync(Expression<Func<TEntity, object>> keySelector, int skip, int take, params string[] includeNavPropPaths)
        {
            return FindAllIncluding(includeNavPropPaths)
                .OrderByDescending(keySelector)
                .Skip(skip)
                .Take(take)
                .ToListAsync(CancellationToken);
        }

        public void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public Task AddRange(params TEntity[] entities)
        {
            return DbSet.AddRangeAsync(entities, CancellationToken);
        }

        public void Update(TEntity entity)
        {
            var dbEntityEntry = MfhDbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Detached)
            {
                DbSet.Attach(entity);
            }
            dbEntityEntry.State = EntityState.Modified;
        }

        public void Delete(TEntity entity)
        {
            var dbEntityEntry = MfhDbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Deleted)
            {
                dbEntityEntry.State = EntityState.Deleted;
            }
            else
            {
                DbSet.Attach(entity);
                DbSet.Remove(entity);
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await FindAllFilteredAsync(predicate);
            if (entities.Any())
            {
                entities.ForEach(Delete);
            }

            await SaveChangesAsync();
        }

        public Task SaveChangesAsync()
        {
            return MfhDbContext.SaveChangesAsync(CancellationToken);
        }

        #region Helpers

        protected IQueryable<TEntity> FindAllIncluding(params string[] includeNavPropPaths)
        {
            IQueryable<TEntity> query = DbSet;
            if (includeNavPropPaths?.Length > 0)
            {
                query = query.IncludeMultiple(includeNavPropPaths);
            }

            return query;
        }

        #endregion Helpers
    }
}