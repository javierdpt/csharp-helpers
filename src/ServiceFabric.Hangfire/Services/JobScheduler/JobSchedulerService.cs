using HangfireService.Infrastructure.Extensions;
using HangfireService.Models;
using ServiceFabric.Infrastructure.Constants;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HangfireService.Services.JobScheduler
{
    public class JobSchedulerService : IJobSchedulerService, IDisposable
    {
        private readonly FabricClient _sfClient;

        public JobSchedulerService(FabricClient sfClient)
        {
            _sfClient = sfClient;
        }

        public async Task<IEnumerable<ApplicationDescriptionDto>> GetApplicationList(bool includeDetails = true)
        {
            var result = new List<ApplicationDescriptionDto>();
            var appList = await _sfClient.QueryManager.GetApplicationListAsync();

            foreach (var app in appList.Where(IsEnvironment))
            {
                var b = new ApplicationDescriptionDto
                {
                    AbsolutePath = app.ApplicationName.AbsolutePath,
                    AbsoluteUri = app.ApplicationName.AbsoluteUri,
                    Services = includeDetails ? await GetServiceList(app) : null
                };

                result.Add(b);
            }

            return result;
        }
        
        public async Task<IEnumerable<ServiceDescriptionDto>> GetServiceList(Application app, bool includeDetails = true)
        {
            var result = new List<ServiceDescriptionDto>();
            var svcList = await _sfClient.QueryManager.GetServiceListAsync(app.ApplicationName);
            var svcTypeDictionary = (await _sfClient.QueryManager.GetServiceTypeListAsync(app.ApplicationTypeName, app.ApplicationTypeVersion))
                .ToDictionary(t => t.ServiceTypeDescription.ServiceTypeName, t => t.ServiceManifestName);

            var svcManifest = new ServiceManifest();
            foreach (var service in svcList)
            {
                var namespaceHint = string.Empty;

                if (svcTypeDictionary.ContainsKey(service.ServiceTypeName))
                {
                    svcManifest = (await _sfClient.ServiceManager.GetServiceManifestAsync(
                            app.ApplicationTypeName,
                            app.ApplicationTypeVersion, svcTypeDictionary[service.ServiceTypeName]))
                        .XmlDeserializeFromString<ServiceManifest>();

                    namespaceHint = svcManifest?.CodePackage?.EntryPoint?.ExeHost?.Program?.Replace(".exe", string.Empty);
                }

                var svc = new ServiceDescriptionDto
                {
                    AbsoluteUri = service.ServiceName.AbsoluteUri,
                    AbsolutePath = service.ServiceName.AbsolutePath,
                    PartitionList = includeDetails ? await GetServicePartitionList(service.ServiceName) : null,
                    NamespaceHint = namespaceHint,
                    Endpoints = svcManifest?.Resources?.Endpoints?.Endpoint
                            ?.Where(e => string.IsNullOrWhiteSpace(e.Protocol) || !e.Protocol.Equals("http", StringComparison.InvariantCultureIgnoreCase))
                            .Select(e => e.Name),
                    SupportedTypes = GetSupportedTypes(namespaceHint, false).Select(t => new TypeDescriptionDto(t))
                };

                result.Add(svc);
            }

            return result;
        }

        public async Task<IEnumerable<ServicePartitionDescriptionDto>> GetServicePartitionList(Uri serviceName)
        {
            var partitionList = await _sfClient.QueryManager.GetPartitionListAsync(serviceName);

            return partitionList.Select(partition =>
                new ServicePartitionDescriptionDto
                {
                    Id = partition.PartitionInformation.Id,
                    Kind = partition.PartitionInformation.Kind
                });
        }

        public async Task<IEnumerable<string>> GetServicePartitionEndpoint(Uri serviceName, Partition partition)
        {
            var resolver = ServicePartitionResolver.GetDefault();

            ServicePartitionKey key;
            switch (partition.PartitionInformation.Kind)
            {
                case ServicePartitionKind.Singleton:
                    key = ServicePartitionKey.Singleton;
                    break;

                case ServicePartitionKind.Int64Range:
                    var longKey = (Int64RangePartitionInformation)partition.PartitionInformation;
                    key = new ServicePartitionKey(longKey.LowKey);
                    break;

                case ServicePartitionKind.Named:
                    var namedKey = (NamedPartitionInformation)partition.PartitionInformation;
                    key = new ServicePartitionKey(namedKey.Name);
                    break;

                default:
                    throw new Exception("Unable to resolve partition kind. Parameter partition.PartitionInformation.Kind was out of range");
            }
            var resolved = await resolver.ResolveAsync(serviceName, key, CancellationToken.None);

            return resolved.Endpoints.Select(e => e.Address);
        }

        public IEnumerable<MethodInfo> GetTypeMethods(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            var typesInfo = GetSupportedTypes(typeName).ToList();

            return typesInfo.Any()
                ? typesInfo.SelectMany(t => t.DeclaredMethods)
                : null;
        }

        public IEnumerable<ParameterInfo> GetMethodArguments(string typeName, string methodName)
        {
            if (string.IsNullOrWhiteSpace(typeName) ||
                string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            var methods = GetTypeMethods(typeName)?.ToList();

            return methods != null && methods.Any(m => m.Name == methodName)
                ? methods.First(m => m.Name == methodName).GetParameters()
                : null;
        }

        public IEnumerable<TypeInfo> GetSupportedTypes()
        {
            return from currentAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                   from assemblyType in Assembly.Load(currentAssembly).DefinedTypes
                   where assemblyType.ImplementedInterfaces.Contains(typeof(IService))
                   select assemblyType;
        }

        private static bool IsEnvironment(Application app)
        {
            return (Startup.AppEnvironment != AppEnvironmentEnum.Dev &&
                    Startup.AppEnvironment != AppEnvironmentEnum.Model) ||
                   app.ApplicationName.AbsoluteUri.Equals(
                       $"fabric:/{Startup.AppEnvironment}{app.ApplicationName.AbsolutePath}",
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private IEnumerable<TypeInfo> GetSupportedTypes(string typeName, bool exactMatch = true)
        {
            return from assemblyType in GetSupportedTypes()
                   where string.IsNullOrWhiteSpace(typeName) ||
                         (
                             assemblyType.FullName != null &&
                             (exactMatch ? assemblyType.FullName == typeName : assemblyType.FullName.Contains(typeName))
                           )
                   select assemblyType;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _sfClient.Dispose();
        }
    }
}