using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GS.MFH.CommunityData.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllAsync(params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredAsync(Expression<Func<TEntity, bool>> predicate, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllOrderByAscAsync(Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredOrderByAscAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllOrderByDescAsync(Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredOrderByDescAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllTakeAsync(int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllAscTakeAsync(Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllDescTakeAsync(Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredTakeAsync(Expression<Func<TEntity, bool>> predicate, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredAscTakeAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllFilteredDescTakeAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> keySelector, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllSkipTakeAscAsync(Expression<Func<TEntity, object>> keySelector, int skip, int take, params string[] includeNavPropPaths);

        Task<List<TEntity>> FindAllSkipTakeDescAsync(Expression<Func<TEntity, object>> keySelector, int skip, int take, params string[] includeNavPropPaths);

        void Add(TEntity entity);

        Task AddRange(params TEntity[] entities);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

        Task SaveChangesAsync();
    }
}