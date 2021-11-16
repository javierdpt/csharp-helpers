using System;
using GS.MFH.RentersInsurance.Api.Infrastructure.Options;
using GS.MFH.RentersInsurance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GS.MFH.RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Application Database access setup
    /// </summary>
    public static class DbSetup
    {
        /// <summary>
        /// Add setup
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dbOptions"></param>
        /// <returns></returns>
        private static void AddAppDatabases(this IServiceCollection services, DbOptions dbOptions)
        {
            services.AddDbContext<RentersInsuranceDbContext>(options =>
            {
                options.UseSqlServer(dbOptions.RentersServicesDb,
                    sqlOpts =>
                    {
                        sqlOpts.MigrationsHistoryTable("__EFMigrationsHistory", "rins");
                        sqlOpts.EnableRetryOnFailure(maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }
                );
            });
            services.AddScoped<IRentersInsuranceDbContext, RentersInsuranceDbContext>();
        }

        /// <summary>
        /// Add setup
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppDatabases(this IServiceCollection services)
        {
            var dbOptions = services
                .BuildServiceProvider()
                .GetService<IOptions<DbOptions>>();

            services.AddAppDatabases(dbOptions.Value);

            return services;
        }
    }
}