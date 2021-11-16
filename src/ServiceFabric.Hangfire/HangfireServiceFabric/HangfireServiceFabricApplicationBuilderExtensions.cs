using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;

namespace HangfireService.Infrastructure.HangfireServiceFabric
{
    public static class HangfireServiceFabricApplicationBuilderExtensions
    {
        /// <summary>
        /// Set up Hangfire on Service Fabric
        /// </summary>
        /// <param name="app"></param>
        /// <param name="sfContext"></param>
        /// <param name="pathMatch"></param>
        /// <param name="ignore"></param>
        /// <param name="options"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHangfireDashboardServiceFabric(
            [NotNull] this IApplicationBuilder app,
            [NotNull] ServiceContext sfContext,
            [NotNull] string pathMatch = "/hangfire",
            [NotNull] List<string> ignore = null,
            [CanBeNull] DashboardOptions options = null,
            [CanBeNull] JobStorage storage = null)
        {
            if (sfContext == null) throw new ArgumentNullException(nameof(app));
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
            if (app.ApplicationServices.GetService<IGlobalConfiguration>() == null)
            {
                throw new InvalidOperationException("Unable to find the required services. Please add all the required services by calling 'IServiceCollection.AddHangfire' inside the call to 'ConfigureServices(...)' in the application startup code.");
            }

            var services = app.ApplicationServices;

            storage = storage ?? services.GetRequiredService<JobStorage>();
            options = options ?? services.GetService<DashboardOptions>() ?? new DashboardOptions();
            var routes = app.ApplicationServices.GetRequiredService<RouteCollection>();

            app.MapWhen(
                context =>
                    context.Request.Path.StartsWithSegments(pathMatch) &&
                    (
                        ignore == null ||
                        ignore.All(i => !context.Request.Path.StartsWithSegments(i)
                    )
                ),
                appBuilder =>
                {
                    appBuilder.UseMiddleware<HangfireServiceFabricDashboardMiddleware>(
                        sfContext.ServiceName.AbsolutePath, storage, options, routes, pathMatch);
                }
            );

            return app;
        }
    }
}