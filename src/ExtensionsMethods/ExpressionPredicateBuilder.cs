﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GS.MFH.Lease.Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// Predicate builder extension methods for And and Or funct
    /// </summary>
    public static class ExpressionPredicateBuilder
    {
        /// <summary>
        /// Expression 'And' extenssion method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>New expression with a and b</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var p = a.Parameters[0];

            var visitor = new SubstExpressionVisitor();
            visitor.Subst[b.Parameters[0]] = p;

            Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body) ?? throw new InvalidOperationException());
            return Expression.Lambda<Func<T, bool>>(body, p);
        }

        /// <summary>
        /// Expression 'Or' extenssion method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>New expression with a || b</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var p = a.Parameters[0];

            var visitor = new SubstExpressionVisitor();
            visitor.Subst[b.Parameters[0]] = p;

            Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body) ?? throw new InvalidOperationException());
            return Expression.Lambda<Func<T, bool>>(body, p);
        }
    }

    internal class SubstExpressionVisitor : ExpressionVisitor
    {
        public Dictionary<Expression, Expression> Subst = new Dictionary<Expression, Expression>();

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression newValue;
            if (Subst.TryGetValue(node, out newValue))
            {
                return newValue;
            }
            return node;
        }
    }
}