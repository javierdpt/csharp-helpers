using Api.Infrastructure.AppSetup;
using Api.Interfaces;
using Api.Services;
using ServiceFabric.Infrastructure.Helpers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ApplicationInsights.ServiceFabric.Module;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;
using NLog.Web;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Api
{
    internal sealed class LeaseApi : StatelessService
    {
        public LeaseApi(StatelessServiceContext context) : base(context)
        {
        }

        #region Overrides

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                ServiceInstanceListenersHelpers.GetRemoteServiceInstanceListener(
                    new LeaseUtilsService(),
                    LeaseUtilsServiceConst.Endpoint,
                    ServiceEventSource.Current.ServiceMessage,
                    LogManager.GetCurrentClassLogger().Fatal
                ),
                ServiceInstanceListenersHelpers.GetRemoteServiceInstanceListener(
                    new LeaseMaintenanceService(),
                    LeaseMaintenanceServiceConst.Endpoint,
                    ServiceEventSource.Current.ServiceMessage,
                    LogManager.GetCurrentClassLogger().Fatal
                ),
                ServiceInstanceListenersHelpers.GetRemoteServiceInstanceListener(
                    new LeaseCommunicationService(),
                    LeaseCommunicationServiceConst.Endpoint,
                    ServiceEventSource.Current.ServiceMessage,
                    LogManager.GetCurrentClassLogger().Fatal
                ),
                ServiceInstanceListenersHelpers.GetRemoteServiceInstanceListener(
                    new LeaseReportingService(),
                    LeaseReportingServiceConst.Endpoint,
                    ServiceEventSource.Current.ServiceMessage,
                    LogManager.GetCurrentClassLogger().Fatal
                ),
                new ServiceInstanceListener(serviceContext =>
                {
                    return new KestrelCommunicationListener(serviceContext, (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                            .UseKestrel()
                            .ConfigureServices(services => services
                                .AddSingleton(serviceContext)
                                .AddSingleton<ITelemetryInitializer>(serviceProvider => FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(serviceContext))
                                .AddSingleton<ITelemetryModule>(new ServiceRemotingDependencyTrackingTelemetryModule())
                                .AddSingleton<ITelemetryModule>(new ServiceRemotingRequestTrackingTelemetryModule())
                            )
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseStartup<Startup>()
                            .ConfigureLogging(builder =>
                                builder.ConfigAppLogger(Program.Configurations.GetSection("Logging")))
                            .UseNLog()
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseUrls(url)
                            .Build();
                    });
                }, "HttpEndpoint")
            };
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            LogManager.GetCurrentClassLogger().Info($"{GetType().Name} {Context.InstanceId} OPEN.");
            return base.OnOpenAsync(cancellationToken);
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            LogManager.GetCurrentClassLogger().Info($"{GetType().Name} {Context.InstanceId} CLOSE.");
            return base.OnCloseAsync(cancellationToken);
        }

        #endregion Overrides
    }
}