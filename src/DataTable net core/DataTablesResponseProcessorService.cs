using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Api.Infrastructure.ExtensionMethods;
using Utilities.Extensions;

namespace Api.Infrastructure.DataTables
{
    /// <summary>
    /// DataTablesResponseProcessor service
    /// </summary>
    public class DataTablesResponseProcessorService : IDataTablesResponseProcessorService
    {
        /// <summary>
        /// Filter IQueryable of TEntity type using the DataTableResquestModel mapping the result items
        /// to TEntityDto
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TEntityDto"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="model"></param>
        /// <param name="entityAllQuery"></param>
        /// <returns>
        /// A DataTablesResponseModel with the list filtered by the model
        /// mapped to TEntityDto
        /// </returns>
        public async Task<DataTablesResponse<TEntityDto>> Filter<TEntity, TEntityDto>(
            IMapper mapper, DataTableRequestModel model, IQueryable<TEntity> entityAllQuery)
            where TEntity : class
            where TEntityDto : class
        {
            var result = await ExecuteDataTableQuery(model, entityAllQuery); //, out int filteredResultsCount, out int totalResultsCount);

            // this is what datatables wants sending back
            return new DataTablesResponse<TEntityDto>
            {
                Draw =  model.Draw,
                RecordsTotal = result.TotalResultsCount,
                RecordsFiltered = result.FilteredResultsCount,
                Data = mapper.Map<IList<TEntity>, IList<TEntityDto>>(result.Items)
            };
        }

        /// <summary>
        /// Filter IQueryable of TEntity type using the DataTableResquestModel
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model"></param>
        /// <param name="entityAllQuery"></param>
        /// <returns>A DataTablesResponseModel with the list filtered by the model</returns>
        public async Task<DataTablesResponse<TEntity>> Filter<TEntity>(
            DataTableRequestModel model, IQueryable<TEntity> entityAllQuery)
            where TEntity : class
        {
            var result = await ExecuteDataTableQuery(model, entityAllQuery); //, out int filteredResultsCount, out int totalResultsCount);

            // this is what datatables wants sending back
            return new DataTablesResponse<TEntity>
            {
                Draw = model.Draw,
                RecordsTotal = result.TotalResultsCount,
                RecordsFiltered = result.FilteredResultsCount,
                Data = result.Items
            };
        }

        private async Task<DataTableDbResult<TEntity>> ExecuteDataTableQuery<TEntity>(
            DataTableRequestModel model, IQueryable<TEntity> entityAllQuery)
            where TEntity : class
        {
            var searchBy = model.Search?.Value;
            var take = model.Length == 0 ? 1000 : model.Length;
            var skip = model.Start;
            var sortBy = BuildDynamicOrderBy(model);
            
            // search the dbase taking into consideration table sorting and paging
            return await GetDataFromDbase(model, entityAllQuery, searchBy, take, skip, sortBy);
        }

        private async Task<DataTableDbResult<TEntity>> GetDataFromDbase<TEntity>(DataTableRequestModel model,
            IQueryable<TEntity> entityAllQuery, string searchBy, int take, int skip, string sortBy)
            where TEntity : class
        {
            var query = entityAllQuery;

            query = BuildDynamicWhereClause(model, searchBy, query);

            if (!string.IsNullOrEmpty(sortBy))
            {
                // if we have an empty search then just order the results by Id ascending
                query = query.OrderBy(sortBy);
            }
            else
            {
                var sortProperty = typeof(TEntity).GetProperties().OrderBy(x => x.Name.Length)
                    .FirstOrDefault(x => x.Name.EndsWith("Id"));
                query = query.OrderBy(sortProperty.Name);
            }

            var resultTask = query
                .Skip(skip)
                .Take(take)
                .ToListAsync();
            var filteredResultsCountTask = query.CountAsync();
            var totalResultsCountTask = entityAllQuery.CountAsync();

            return new DataTableDbResult<TEntity>
            {
                Items = await resultTask,
                FilteredResultsCount = await filteredResultsCountTask,
                TotalResultsCount = await totalResultsCountTask
            };
        }

        private static IQueryable<TEntity> BuildDynamicWhereClause<TEntity>(DataTableRequestModel model, string searchBy, IQueryable<TEntity> query) where TEntity : class
        {
            if (string.IsNullOrEmpty(searchBy)) return query;

            var expressions = new List<System.Linq.Expressions.Expression<Func<TEntity, bool>>>();

            model.Columns.ForEach(col =>
            {
                var propertyName = col.Data.FirstCharToUpper();
                var propertyInfo = typeof(TEntity).GetProperty(propertyName);

                if (propertyInfo == null) return;

                expressions.Add(propertyInfo.PropertyType != typeof(string)
                    ? ExpressionComparisons.SimpleComparison<TEntity>(propertyName, searchBy)
                    : ExpressionComparisons.PropertyContains<TEntity, string>(propertyInfo, searchBy));
            });

            var first = expressions.FirstOrDefault();
            var count = 0;
            var whereExpression = expressions.Aggregate(first, (curr, next) => 
                count++ == 0 ? curr : curr.Or(next));

            if (expressions.Count > 0)
            {
                query = query.Where(whereExpression);
            }

            return query;
        }

        private static string BuildDynamicOrderBy(DataTableRequestModel model)
        {
            var sortBy = "";
            if (model.Order == null)
                return sortBy;

            for (var i = 0; i < model.Order.Count; i++)
            {
                if (i == 0)
                    sortBy += model.Columns[model.Order[i].Column].Data + " " + model.Order[i].Dir.ToLower();
                else
                    sortBy += ", " + model.Columns[model.Order[i].Column].Data + " " + model.Order[i].Dir.ToLower();
            }

            return sortBy;
        }
    }

    internal class DataTableDbResult<TEntityDto> where TEntityDto : class
    {   
        public IList<TEntityDto> Items { get; set; }
        public int FilteredResultsCount { get; set; }
        public int TotalResultsCount { get; set; }
    }
}