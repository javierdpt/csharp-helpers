using .RentersInsurance.Api.Infrastructure.Options;
using .ServiceFabric.Infrastructure.ProxyResolver.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace .RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Add application setup
    /// </summary>
    public static class OptionsSetup
    {
        /// <summary>
        /// Add application options
        /// </summary>
        public static IServiceCollection AddAppOptions(
            this IServiceCollection services,
            IConfigurationRoot configuration,
            IDictionary<string, string> settings)
        {
            services.AddOptions();

            services.Configure<AppOptions>(configuration);
            services.Configure<DbOptions>(configuration.GetSection("ConnectionStrings"));
            services.Configure<DbOptions>(options =>
            {
                options.MultifamilyHousing = string.Format(options.MultifamilyHousing, settings["MfhDbUserName"], settings["MfhDbPassword"]);
                options.MultifamilyHousingAnalytics = string.Format(options.MultifamilyHousingAnalytics, settings["MfhDbUserName"], settings["MfhDbPassword"]);
                options.RentersServicesDb = string.Format(options.RentersServicesDb, settings["RentersServicesDbUserName"], settings["RentersServicesDbPwd"]);
            });

            services.Configure<RemotingIntegrationOptions>(configuration.GetSection("Integrations:Remoting"));
            services.Configure<HttpIntegrationOptions>(configuration.GetSection("Integrations:Http"));
            services.Configure<ProxyResolverConfig>(options =>
            {
                var opt = configuration
                    .GetSection("Integrations:Remoting:EmailService")
                    .Get<RemotingIntegrationServiceOption>();
                options.BaseUri = opt.BaseUri;
                options.ServiceName = opt.ServiceName;
                options.ServiceListenerName = opt.ServiceListenerName;
            });

            return services;
        }
    }
}