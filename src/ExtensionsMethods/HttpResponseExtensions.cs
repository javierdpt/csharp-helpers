using Microsoft.AspNetCore.Http;
using System.Net;

namespace .RentersInsurance.Api.Infrastructure.Extensions
{
    /// <summary>
    /// HttpResponse extension methods
    /// </summary>
    public static class HttpResponseExtensions
    {
        /// <summary>
        /// Returns a cookie from a response by reading the headers
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookieValue(this HttpResponse response, string cookieName)
        {
            foreach (var headers in response.Headers.Values)
                foreach (var header in headers)
                    if (header.StartsWith($"{cookieName}="))
                    {
                        var p1 = header.IndexOf('=');
                        var p2 = header.IndexOf(';');
                        return WebUtility.UrlDecode(header.Substring(p1 + 1, p2 - p1 - 1));
                    }
            return null;
        }
    }
}