using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// Expression comparison methods for search in entity by property as string
    /// </summary>
    public static class ExpressionComparisons
    {
        /// <summary>
        /// Simple comparison extension method
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>Expression</returns>
        public static Expression<Func<TSource, bool>> SimpleComparison<TSource>
            (string property, object value)
        {
            var type = typeof(TSource);
            var pe = Expression.Parameter(type, "p");
            var propertyReference = Expression.Property(pe, property);
            var valueAsPropertyType = Convert.ChangeType(value, propertyReference.Type);
            var constantReference = Expression.Constant(valueAsPropertyType);
            return Expression.Lambda<Func<TSource, bool>>
            (Expression.Equal(propertyReference, constantReference),
                new[] { pe });
        }

        /// <summary>
        /// Property containis comparison extension method
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TPropery"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression<Func<TParent, bool>> PropertyContains<TParent, TPropery>(PropertyInfo property, TPropery value)
        {
            var parent = Expression.Parameter(typeof(TParent));
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(TPropery) });
            var expressionBody = Expression.Call(Expression.Property(parent, property), method, Expression.Constant(value));
            return Expression.Lambda<Func<TParent, bool>>(expressionBody, parent);
        }
    }
}