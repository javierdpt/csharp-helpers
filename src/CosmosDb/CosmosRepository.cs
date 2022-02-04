using JsonFlatten;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Repository
{
    public abstract class CosmosRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected Container Container { get; }

        protected ICosmosLinqQuery CosmosLinqQuery { get; }

        protected CosmosRepository(
            Container container,
            ICosmosLinqQuery cosmosLinqQuery
        )
        {
            Container = container;
            CosmosLinqQuery = cosmosLinqQuery;
        }

        protected CosmosRepository(
            Container container,
            ICosmosLinqQuery cosmosLinqQuery,
            JsonSerializer internalJsonSerializer
        )
        {
            Container = container;
            CosmosLinqQuery = cosmosLinqQuery;
            _internalJsonSerializer = internalJsonSerializer;
        }

        public virtual async Task<TEntity> AddDocumentAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        )
        {
            var partitionKey = GetPartitionKey(entity);

            var cosmosResponse = await Container
                .CreateItemAsync(entity, partitionKey, null, cancellationToken)
                .ConfigureAwait(false);

            return cosmosResponse.Resource;
        }

        public virtual async Task<TEntity> GetDocumentByIdAsync(
            string id,
            CancellationToken cancellationToken = default
        ) => (
                await Container
                    .ReadItemAsync<TEntity>(id, GetPartitionKey(id), null, cancellationToken)
                    .ConfigureAwait(false)
            )
            .Resource;

        protected virtual async Task<IEnumerable<TEntity>> ExecuteQueryAsync(
            QueryDefinition qd,
            PartitionKey partitionKey,
            CancellationToken cancellationToken
        )
        {
            var itr = Container.GetItemQueryIterator<TEntity>(
                qd,
                null,
                new QueryRequestOptions { PartitionKey = partitionKey });

            var results = new List<TEntity>();
            while (itr.HasMoreResults)
            {
                results.AddRange(await itr.ReadNextAsync(cancellationToken).ConfigureAwait(false));
            }

            return results;
        }

        public virtual async Task DeleteDocumentAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        ) => await Container.DeleteItemAsync<TEntity>(
                entity!.Id, GetPartitionKey(entity), cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            string partitionKey = null,
            CancellationToken cancellationToken = default
        )
        {
            QueryRequestOptions requestOptions = null;
            if (partitionKey != null)
            {
                requestOptions = new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                };
            }

            return await CosmosLinqQuery.GetFeedIterator(
                    Container.GetItemLinqQueryable<TEntity>(requestOptions: requestOptions).Where(predicate))
                .IterateAllResultsAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual Task UpdateDocumentAsync(
            TEntity entity,
            bool matchTag = false,
            CancellationToken cancellationToken = default
        )
        {
            return Container.ReplaceItemAsync(
                entity,
                entity!.Id,
                GetPartitionKey(entity),
                matchTag ? new ItemRequestOptions { IfMatchEtag = entity.ETag } : null,
                cancellationToken
            );
        }

        public async Task<TEntity> UpsertAsync(
            TEntity entity,
            CancellationToken cancellationToken = default
        )
        {
            var partitionKey = GetPartitionKey(entity);

            var cosmosResponse = await Container
                .UpsertItemAsync(entity, partitionKey, null, cancellationToken)
                .ConfigureAwait(false);

            return cosmosResponse.Resource;
        }

        public virtual async Task<IEnumerable<TEntity>> UpsertAsBatchAsync(
            IEnumerable<TEntity> entities,
            string partitionKey,
            CancellationToken cancellationToken = default
        )
        {
            var batch = Container.CreateTransactionalBatch(GetPartitionKey(partitionKey));

            var items = entities.ToList();
            foreach (TEntity entity in items)
                batch.UpsertItem(entity);

            var batchResponse = await batch.ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            using (batchResponse)
            {
                if (!batchResponse.IsSuccessStatusCode)
                    throw new DataMisalignedException(batchResponse.ErrorMessage);

                var result = new List<TEntity>();
                for (var i = 0; i < items.Count; i++)
                {
                    var resp = batchResponse.GetOperationResultAtIndex<TEntity>(i);
                    result.Add(resp.Resource);
                }

                return result;
            }
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> PatchDocumentAsync(
            string id,
            object partialEntity,
            string partitionKey = null,
            CancellationToken cancellationToken = default
        )
        {
            var entityPartitionKey = !string.IsNullOrWhiteSpace(partitionKey)
                ? GetPartitionKey(partitionKey)
                : GetPartitionKey(); // This can cause an issue if the entity doesn't have a parameter-less ctor

            // Read raw item from the DB to skip TEntity serialization to avoid set of default prop value
            var currentEntity = await Container
                .ReadItemAsync<JObject>(id, entityPartitionKey, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (currentEntity is null) return null;

            var entityToPatchJObject = currentEntity.Resource;
            var entityToPatchFlattened = PatchGetEntityFlattenedProps(entityToPatchJObject);
            var partialEntityJObject = JObject.FromObject(partialEntity, _internalJsonSerializer);
            var partialEntityFlattened = PatchGetEntityFlattenedProps(partialEntityJObject, true);

            if (!partialEntityFlattened.Any()) throw new ArgumentNullException(nameof(partialEntity));

            return await Container.PatchItemAsync<TEntity>(
                id, entityPartitionKey,
                partialEntityFlattened
                    .Select(kvp => (key: PatchCorrectArrayKeyPath(kvp.Key), value: kvp.Value))
                    .GroupBy(kvp => kvp.key)
                    .Select(grp => (partialEntityKey: grp.Key, partialEntityValue: grp.First().value))
                    .Select(kv =>
                        PatchRemoveOperation(kv.partialEntityValue, kv.partialEntityKey) ??
                        PatchUpdateOperation(entityToPatchFlattened, kv.partialEntityValue, kv.partialEntityKey) ??
                        PatchAddOperation(partialEntityJObject, entityToPatchJObject, kv.partialEntityKey)
                    )
                    .ToList(),
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        public virtual PartitionKey GetPartitionKey(TEntity entity) => GetPartitionKey(
            !string.IsNullOrWhiteSpace(entity!.PartitionKey)
                ? entity.PartitionKey
                : entity.Id
        );

        public PartitionKey GetPartitionKey(string partitionKey) => new PartitionKey(partitionKey);

        /// <summary>
        /// Get TEntity partition key by activating parameter-less constructor
        /// and getting PartitionKey property (This case is for Entities with
        /// static partitionKey and parameter-less constructor
        /// </summary>
        /// <returns></returns>
        public PartitionKey GetPartitionKey() => GetPartitionKey(Activator.CreateInstance<TEntity>());

        #region Helpers

        private static string PatchCorrectArrayKeyPath(string key) => !key.Contains('[') && !key.Contains(']')
            ? key
            : key.Substring(0, key.IndexOf('[')); // Arrays (Updating entire nested object)

        private PatchOperation PatchRemoveOperation(object partialEntityValue, string partialEntityKey) =>
            partialEntityValue == null
                ? PatchOperation.Remove(JsonPathToPatchPath(partialEntityKey))
                : null;

        private PatchOperation PatchUpdateOperation(
            IReadOnlyDictionary<string, object> entityToPatchFlattened,
            object partialEntityValue,
            string partialEntityKey
        ) => entityToPatchFlattened.ContainsKey(partialEntityKey)
            ? PatchOperation.Set(JsonPathToPatchPath(partialEntityKey), partialEntityValue)
            : null;

        private PatchOperation PatchAddOperation(
            JObject partialEntityJObject,
            JObject entityToPatchJObject,
            string partialEntityKey
        )
        {
            var key = PatchGetAddKey(partialEntityKey, entityToPatchJObject);
            return PatchOperation.Add(
                JsonPathToPatchPath(key),
                partialEntityJObject.SelectToken(key)
            );
        }

        /// <summary>
        /// Patch Find Add key Token. Searching for the closes nested initialized value
        /// to avoid https://stackoverflow.com/questions/70596723/cosmos-db-add-item-in-non-existent-parent-using-patch/70604564
        /// issue
        /// </summary>
        /// <param name="partialEntityKey"></param>
        /// <param name="entityToPatchJObject"></param>
        /// <returns></returns>
        protected string PatchGetAddKey(string partialEntityKey, JObject entityToPatchJObject)
        {
            var individualTokens = partialEntityKey!.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var individualPathLength = individualTokens.Length;

            var i = 0;
            var key = new StringBuilder(individualTokens[0]);
            while (
                ++i < individualPathLength &&
                entityToPatchJObject.SelectToken(key.ToString()) != null
            ) key.Append(".").Append(individualTokens[i]);

            return key.ToString();
        }

        protected Dictionary<string, object> PatchGetEntityFlattenedProps(
            JObject entityJObject, bool bypassNull = false
        ) => entityJObject
            ?.Flatten()
            .Where(kvp => !_entityIgnoredProps.Contains(kvp.Key) && (bypassNull || kvp.Value != null))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        protected string JsonPathToPatchPath(string jsonPath) => $"/{jsonPath!.Replace('.', '/')}";

        #endregion Helpers

        private readonly JsonSerializer _internalJsonSerializer = new JsonSerializer
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }
            }
        };

        private readonly HashSet<string> _entityIgnoredProps = new HashSet<string>
        {
            "id", "partitionKey", "documentType", "_rid", "_self", "_etag", "_attachments", "_ts"
        };
    }
}
