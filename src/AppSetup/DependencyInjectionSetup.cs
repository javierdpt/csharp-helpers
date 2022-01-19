using Common.Api;
using Common.BackgroundJobs;
using RentersInsurance.Api.Services.ApplicationService;
using RentersInsurance.Api.Services.AppResources;
using RentersInsurance.Api.Services.AudCompliance;
using RentersInsurance.Api.Services.BillingService;
using RentersInsurance.Api.Services.Client;
using RentersInsurance.Api.Services.Communication;
using RentersInsurance.Api.Services.Maintenance;
using RentersInsurance.Api.Services.Moratorium;
using RentersInsurance.Api.Services.Places;
using RentersInsurance.Api.Services.Policy;
using RentersInsurance.Api.Services.ProxyResolver;
using RentersInsurance.Api.Services.RecurrentJobs;
using RentersInsurance.Api.Services.Reports;
using RentersInsurance.Api.Services.Token;
using RentersInsurance.Api.Services.Tracking;
using RentersInsurance.Api.Services.UwQuestions;
using RentersInsurance.Data;
using ServiceFabric.Common.Clients.EmailClientService;
using ServiceFabric.Common.Clients.ProxyResolverService;
using ServiceFabric.Infrastructure.ProxyResolver.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// App dependency injection setup
    /// </summary>
    public static class DependencyInjectionSetup
    {
        /// <summary>
        /// Add app dependencies definitions
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<IRentersInsuranceDbContext, RentersInsuranceDbContext>();
            services.AddScoped<IPlacesService, PlacesService>();
            services.AddScoped<IEmailClientService, EmailClientService>(
                provider => new EmailClientService(provider.GetService<IProxyResolverClientService>())
            );
            services.AddScoped<IUwqResourcesService, UwqResourcesService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IRecurrentJobsService, RecurrentJobsService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IAudComplianceService, AudComplianceService>();
            services.AddScoped<IReportService, ReportService>();

            services.AddSingleton<ICommunicationService, CommunicationService>();
            services.AddSingleton<IMoratoriumService, Services.Moratorium.MoratoriumService>();
            services.AddSingleton<IProxyResolverService, ProxyResolverService>();
            services.AddSingleton<ISimpleBgWorker, SimpleBgWorker>();
            services.AddSingleton<IApiFactory, ApiFactory>();
            services.AddSingleton<IBillingService, BillingService>();
            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IProxyResolverClientService, ProxyResolverClientService>(
                provider => new ProxyResolverClientService(
                    provider.GetService<ILogger<ProxyResolverClientService>>(),
                    provider.GetService<IOptions<ProxyResolverConfig>>())
            );
            services.AddSingleton<IAppResourcesService, AppResourcesService>();
            services.AddSingleton<ITrackingSvc, TrackingSvc>();
            services.AddSingleton<IClientService, ClientService>();

            return services;
        }
    }
}