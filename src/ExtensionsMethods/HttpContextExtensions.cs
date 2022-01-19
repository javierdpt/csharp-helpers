using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// HttpContext Extensions methods
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the ClaimTypes.NameIdentifier value or "Unknown"
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetNameIdentifierClaim(this HttpContext httpContext)
        {
            return httpContext == null
                ? "System"
                : httpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        }
    }
}