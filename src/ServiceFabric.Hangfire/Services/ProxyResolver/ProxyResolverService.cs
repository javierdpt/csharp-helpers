using ConfigManager.Interfaces;
using ServiceFabric.Infrastructure.ProxyResolver;
using ServiceFabric.JsonSerializer;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;

namespace HangfireService.Services.ProxyResolver
{
    public class ProxyResolverService : ProxyResolverServiceBase, IProxyResolverService
    {
        public ProxyResolverService(ILogger<ProxyResolverService> logger)
            : base(logger)
        {
            ProxyFactoryJsonSerializer = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory(
                new FabricTransportRemotingSettings
                {
                    MaxMessageSize = ProxyMaxMessageSize,
                    OperationTimeout = TimeSpan.FromMinutes(60)
                },
                serializationProvider: new ServiceRemotingJsonSerializationProvider()
            ));
            ProxyFactoryDefaultSerializer = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory(
                new FabricTransportRemotingSettings
                {
                    MaxMessageSize = ProxyMaxMessageSize,
                    OperationTimeout = TimeSpan.FromMinutes(60)
                }
            ));
        }

        public IConfigManagerService ConfigManagerService()
        {
            return CreateServiceJsonSerializer<IConfigManagerService>(
                $"{Startup.Configuration["Integrations:Remoting:Services:BaseUri"]}/{Startup.Configuration["Integrations:Remoting:Services:ConfigManagerServiceName"]}"
            );
        }

        public IService GetServiceJsonSerializer(
            Type serviceInterfaceType,
            string uri,
            string listenerName = null,
            object partitionKey = null)
        {
            Logger.LogDebug($"Getting remote service with JsonSerializer {uri} - {serviceInterfaceType.FullName}.");

            return GetServiceByType(ProxyFactoryJsonSerializer, serviceInterfaceType, uri, listenerName, partitionKey);
        }

        public IService GetServiceDefaultSerializer(
            Type serviceInterfaceType,
            string uri,
            string listenerName = null)
        {
            Logger.LogDebug($"Getting remote service with DefaultSerializer {uri} - {serviceInterfaceType.FullName}.");
            return GetServiceByType(ProxyFactoryDefaultSerializer, serviceInterfaceType, uri, listenerName);
        }

        #region Helpers

        private IService GetServiceByType(
            ServiceProxyFactory serviceProxyFactory,
            Type serviceInterfaceType,
            string uri,
            string listenerName,
            object partitionKey = null)
        {
            try
            {
                var method = typeof(ServiceProxyFactory).GetMethod(nameof(ServiceProxyFactory.CreateServiceProxy));
                var genericMethodToExecute = method.MakeGenericMethod(serviceInterfaceType);

                var partKey = partitionKey is long
                        ? new ServicePartitionKey((long)partitionKey)
                        : partitionKey is string
                            ? new ServicePartitionKey((string)partitionKey)
                            : null;

                return genericMethodToExecute.Invoke(
                    serviceProxyFactory,
                    new object[]
                    {
                        new Uri(uri),
                        partKey,
                        TargetReplicaSelector.Default,
                        listenerName
                    }
                ) as IService;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, $"Error! Could not create service proxy for \"{uri}\" - \"{serviceInterfaceType.FullName}\" (Error: {e.Message}).");
                throw;
            }
        }

        #endregion Helpers
    }
}