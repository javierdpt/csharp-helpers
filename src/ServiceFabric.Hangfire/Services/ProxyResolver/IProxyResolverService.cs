using ConfigManager.Interfaces;
using ServiceFabric.Infrastructure.ProxyResolver;
using Microsoft.ServiceFabric.Services.Remoting;
using System;

namespace HangfireService.Services.ProxyResolver
{
    public interface IProxyResolverService : IProxyResolverServiceBase
    {
        IService GetServiceJsonSerializer(Type serviceInterfaceType, string uri, string listenerName = null, object partitionKey = null);

        IService GetServiceDefaultSerializer(Type serviceInterfaceType, string uri, string listenerName = null);

        IConfigManagerService ConfigManagerService();
    }
}