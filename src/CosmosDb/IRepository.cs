using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Get Cosmos document from Cosmos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetDocumentByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<T> AddDocumentAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteDocumentAsync(T entity, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            string partitionKey = null,
            CancellationToken cancellationToken = default
        );

        Task UpdateDocumentAsync(T entity, bool matchTag = false, CancellationToken cancellationToken = default);

        Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> UpsertAsBatchAsync(
            IEnumerable<T> entities,
            string partitionKey,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Patch partial entity. Only pass the nested properties to be updated
        /// and to remove a property value, pass the property value set to null.
        /// Arrays are always updated replacing the entire value (As of 2/1/22)
        /// If the entity doesn't have a static partition key pass the value in
        /// the partition key parameter.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partialEntity"></param>
        /// <param name="partitionKey">
        /// Optional value if the entity partition key is static also in that
        /// case be sure the entity has a parameter-less constructor.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns>The updated entity</returns>
        Task<T> PatchDocumentAsync(
            string id,
            object partialEntity,
            string partitionKey = null,
            CancellationToken cancellationToken = default
        );
    }
}
