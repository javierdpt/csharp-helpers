using HangfireService.Infrastructure.Extensions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace HangfireService.Infrastructure.HangfireServiceFabric
{
    /// <summary>
    /// Hangfire Service Fabric DashboardMiddleware
    /// </summary>
    public class HangfireServiceFabricDashboardMiddleware
    {
        private readonly string _sfBaseAppPath;
        private readonly RequestDelegate _next;
        private readonly JobStorage _storage;
        private readonly DashboardOptions _options;
        private readonly RouteCollection _routes;
        private readonly string _pathMatch;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="sfBaseAppPath"></param>
        /// <param name="next"></param>
        /// <param name="storage"></param>
        /// <param name="options"></param>
        /// <param name="routes"></param>
        /// <param name="pathMatch"></param>
        public HangfireServiceFabricDashboardMiddleware(
            [NotNull] string sfBaseAppPath,
            [NotNull] RequestDelegate next,
            [NotNull] JobStorage storage,
            [NotNull] DashboardOptions options,
            [NotNull] RouteCollection routes,
            [NotNull] string pathMatch)
        {
            _sfBaseAppPath = sfBaseAppPath;
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _routes = routes ?? throw new ArgumentNullException(nameof(routes));
            _pathMatch = pathMatch;
        }

        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                httpContext.Request.PathBase = $"{_sfBaseAppPath}{httpContext.Request.PathBase}{_pathMatch}";
                var context = new AspNetCoreDashboardContext(_storage, _options, httpContext);
                var findResult = _routes.FindDispatcher(
                    !string.IsNullOrWhiteSpace(_pathMatch)
                        ? httpContext.Request.Path.Value.Replace(_pathMatch, "")
                        : httpContext.Request.Path.Value
                );

                if (findResult == null)
                {
                    await _next.Invoke(httpContext);
                    return;
                }

                foreach (var filter in _options.Authorization)
                {
                    if (filter.Authorize(context))
                        continue;
                    return;
                }

                context.UriMatch = findResult.Item2;
                await findResult.Item1.Dispatch(context);
            }
            catch (UnauthorizedAccessException e)
            {
                await httpContext.GetHandleUnauthorizedResponse(e);
            }
        }
    }
}