using HangfireService.Infrastructure.CustomEmbeddedCssResources;
using HangfireService.Infrastructure.HangfireServiceFabric;
using HangfireService.Interfaces;
using ServiceFabric.Infrastructure.Constants;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HangfireService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HangfireService.Infrastructure.AppSetup
{
    public static class HangfireSetup
    {
        public static IServiceCollection AddHangfireServer(this IServiceCollection services)
        {
            var DbConnectionString =
                string.Format(Startup.Configuration.GetConnectionString("ConnStrName"), Startup.Settings["DbUserName"], Startup.Settings["DbPassword"]);
            var localHangfireDb =
                Startup.Configuration.GetConnectionString("LocalHangfire");

            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(
                    Startup.AppEnvironment == AppEnvironmentEnum.Local ? localHangfireDb : DbConnectionString,
                    new SqlServerStorageOptions
                    {
                        SchemaName = "JobsHangFire",
                        PrepareSchemaIfNecessary = Startup.AppEnvironment == AppEnvironmentEnum.Local,
                        QueuePollInterval = TimeSpan.FromSeconds(10)
                    });
                config.AddCustomEmbeddedCssResources("HangfireService.Resources.styles-overrides.css");
            });

            return services;
        }

        public static IApplicationBuilder UseHangfire(this IApplicationBuilder app, ServiceContext context)
        {
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromMinutes(1),
                Queues = new[]
                {
                    QueueNamesEnum.integration.ToString(),
                    QueueNamesEnum.lease.ToString(),
                    QueueNamesEnum.@default.ToString()
                },
            });

            app.UseHangfireDashboardServiceFabric(
                context,
                "/Dashboard",
                new List<string> { "/api", "/Scheduler" },
                new DashboardOptions
                {
                    StatsPollingInterval = 5000,
                    AppPath = context.ServiceName.AbsolutePath
                    
                    // Using now AppAuthMiddleware
                    // Authorization = new[] { new HangfireAuthorizationFilter() },
                }
            );

            return app;
        }
    }
}