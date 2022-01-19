using System;
using System.Linq;

namespace .Jobs.HangfireService.Infrastructure.Extensions
{
    public static class TypeExtensions
    {
        public static string GetGenericType(this Type parameterType)
        {
            var typeName = parameterType.IsGenericType
                ? parameterType.Name.Substring(0, parameterType.Name.IndexOf('`'))
                : parameterType.Name;

            if (parameterType.IsGenericType)
            {
                typeName = $"{typeName}<{string.Join(",", parameterType.GenericTypeArguments.Select(GetGenericType))}>";
            }

            return typeName;
        }
    }
}