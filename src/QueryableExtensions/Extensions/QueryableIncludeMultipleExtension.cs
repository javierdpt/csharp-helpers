using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace .Lease.Data.Infrastructure.Extensions
{
    public static class QueryableIncludeMultipleExtension
    {
        public static IQueryable<T> IncludeMultiple<T>(
            this IQueryable<T> query, 
            params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }
    }
}
