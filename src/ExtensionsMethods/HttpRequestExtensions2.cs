using Microsoft.AspNetCore.Http;
using System.Linq;

namespace GS.MFH.RentersInsurance.Api.Infrastructure.Extensions
{
    /// <summary>
    /// HttpRequest extensions methods
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Returns user's ip address by reading the "X-Forwarded-For" header
        /// and taking the first value
        /// </summary>
        /// <returns></returns>
        public static string GetUserIpAddress(this HttpRequest request)
        {
            return request.Headers["X-Forwarded-For"].FirstOrDefault()?.Trim();
        }
    }
}