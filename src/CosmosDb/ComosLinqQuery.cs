using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Assurant.Azure.Data.Cosmos
{
    public interface ICosmosLinqQuery
    {
        FeedIterator<T> GetFeedIterator<T>(IQueryable<T> query);
    }

    public class CosmosLinkQuery : ICosmosLinqQuery
    {
        public FeedIterator<T> GetFeedIterator<T>(IQueryable<T> query)
        {
            return query.ToFeedIterator();
        }
    }
}
