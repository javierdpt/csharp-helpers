using System.Linq;
using System.Reflection;

namespace GS.MFH.RentersInsurance.Api.Infrastructure.Extensions
{
    /// <summary>
    /// Object extension methods
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns query string from object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static string GetHttpRequestQueryString<T>(this T self, params string[] ignore) where T : class
        {
            var type = self.GetType();
            return string.Join(
                "&",
                (
                    from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    where !ignore.Contains(pi.Name)
                    let name = pi.Name
                    let value = type.GetProperty(pi.Name).GetValue(self, null)
                    select $"{name}={value}"
                ).ToArray()
            );
        }
    }
}