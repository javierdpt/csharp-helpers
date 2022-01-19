using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Api.Infrastructure.DataTables
{
    /// <summary>
    /// IDataTablesResponseProcessorService contract
    /// </summary>
    public interface IDataTablesResponseProcessorService
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
        Task<DataTablesResponse<TEntityDto>> Filter<TEntity, TEntityDto>(
            IMapper mapper, DataTableRequestModel model, IQueryable<TEntity> entityAllQuery)
            where TEntity : class
            where TEntityDto : class;

        /// <summary>
        /// Filter IQueryable of TEntity type using the DataTableResquestModel
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="model"></param>
        /// <param name="entityAllQuery"></param>
        /// <returns>A DataTablesResponseModel with the list filtered by the model</returns>
        Task<DataTablesResponse<TEntity>> Filter<TEntity>(
            DataTableRequestModel model, IQueryable<TEntity> entityAllQuery)
            where TEntity : class;
    }
}
