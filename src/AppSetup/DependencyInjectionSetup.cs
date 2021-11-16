using GS.MFH.Common.Api;
using GS.MFH.Common.BackgroundJobs;
using GS.MFH.RentersInsurance.Api.Services.ApplicationService;
using GS.MFH.RentersInsurance.Api.Services.AppResources;
using GS.MFH.RentersInsurance.Api.Services.AudCompliance;
using GS.MFH.RentersInsurance.Api.Services.BillingService;
using GS.MFH.RentersInsurance.Api.Services.Client;
using GS.MFH.RentersInsurance.Api.Services.Communication;
using GS.MFH.RentersInsurance.Api.Services.Maintenance;
using GS.MFH.RentersInsurance.Api.Services.Moratorium;
using GS.MFH.RentersInsurance.Api.Services.Places;
using GS.MFH.RentersInsurance.Api.Services.Policy;
using GS.MFH.RentersInsurance.Api.Services.ProxyResolver;
using GS.MFH.RentersInsurance.Api.Services.RecurrentJobs;
using GS.MFH.RentersInsurance.Api.Services.Reports;
using GS.MFH.RentersInsurance.Api.Services.Token;
using GS.MFH.RentersInsurance.Api.Services.Tracking;
using GS.MFH.RentersInsurance.Api.Services.UwQuestions;
using GS.MFH.RentersInsurance.Data;
using GS.MFH.ServiceFabric.Common.Clients.EmailClientService;
using GS.MFH.ServiceFabric.Common.Clients.ProxyResolverService;
using GS.MFH.ServiceFabric.Infrastructure.ProxyResolver.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.MFH.RentersInsurance.Api.Infrastructure.AppSetup
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