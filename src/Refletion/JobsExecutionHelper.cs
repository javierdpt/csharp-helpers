using Jobs.HangfireService.Infrastructure.Attributes;
using Jobs.HangfireService.Services.ProxyResolver;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Jobs.HangfireService.Infrastructure.Converters;

namespace Jobs.HangfireService.Infrastructure.JobExecutionHelper
{
    public static class JobsExecutionHelper
    {
        [ProlongExpirationTime(3)]
        [CustomDisplayJobName("{0}:{2}:{4}", new[] { "fabric:", "Endpoint" }, new[] { "sf:", "" })]
        public static async Task ExecuteRemoting(
            string uri,
            string serviceAssemblyQualifiedName,
            string listenerName,
            object partitionKey,
            string methodName,
            object[] parameters)
        {
            var serviceInterfaceType = GetServiceInterfaceTypeByAssemblyQualifiedName(serviceAssemblyQualifiedName);
            await Execute(serviceInterfaceType, uri, listenerName, partitionKey, methodName, parameters);
        }

        [ProlongExpirationTime(3)]
        [CustomDisplayJobName("{0}:{2}:{4}", new[] { "fabric:", "Endpoint" }, new[] { "sf:", "" })]
        public static async Task ExecuteRemotingV2(
            string uri,
            string serviceFullName,
            string listenerName,
            object partitionKey,
            string methodName,
            object[] parameters)
        {
            var serviceInterfaceType = GetServiceInterfaceTypeByFullName(serviceFullName);
            await Execute(serviceInterfaceType, uri, listenerName, partitionKey, methodName, parameters);
        }

        [CustomDisplayJobName("{0}:{2}:{3} (Enqueue)", new[] { "fabric:", "Endpoint" }, new[] { "sf:", "" })]
        public static void EnqueueRemoting(
            string uri,
            string serviceAssemblyQualifiedName,
            string listenerName,
            string methodName,
            object[] parameters)
        {
            BackgroundJob.Enqueue(() => ExecuteRemoting(
                uri,
                serviceAssemblyQualifiedName,
                listenerName,
                null,
                methodName,
                parameters
            ));
        }

        #region Helpers

        private static async Task Execute(Type serviceInterfaceType,
            string uri,
            string listenerName,
            object partitionKey,
            string methodName,
            object[] parameters)
        {
            try
            {
                var proxyResolverService = Startup.ServiceProvider.GetService<IProxyResolverService>();
                var service = proxyResolverService.GetServiceJsonSerializer(serviceInterfaceType,
                                                                            uri,
                                                                            listenerName,
                                                                            partitionKey);

                if (service == null)
                {
                    var errMsg = $"Error! Could not find remoting service for \"{serviceInterfaceType.FullName}\".";
                    LogManager.GetCurrentClassLogger().Error(errMsg);
                    throw new Exception(errMsg);
                }

                var proxyMethod = serviceInterfaceType.GetMethod(methodName);

                if (proxyMethod == null)
                {
                    var errMsg =
                        $"Error! Could not find the method \"{methodName}\" in service \"{serviceInterfaceType.FullName}\".";
                    LogManager.GetCurrentClassLogger().Error(errMsg);
                    throw new Exception(errMsg);
                }

                // Fix Int32 as Int64 conversion in Newtonsoft.Json
                parameters = StaticConverters.ConvertInt64ToInt32(parameters);
                // Converts JArray to List
                parameters = StaticConverters.ConvertJArrayToList(parameters);
                var resp = (Task)proxyMethod.Invoke(service, parameters);

                if (resp != null)
                {
                    await resp;
                }
                else
                {
                    LogManager
                        .GetCurrentClassLogger()
                        .Error($"Error! Could not get any response \"{methodName}\" of \"{serviceInterfaceType.FullName}\".");
                }
            }
            catch (Exception e)
            {
                LogManager
                    .GetCurrentClassLogger()
                    .Fatal(e, $"Fatal! Executing \"{methodName}\" of \"{serviceInterfaceType.AssemblyQualifiedName}\" (Error: {e.Message}).");
                throw;
            }
        }

        private static Type GetServiceInterfaceTypeByAssemblyQualifiedName(string serviceAssemblyQualifiedName)
        {
            var serviceInterfaceType = Type.GetType(serviceAssemblyQualifiedName);
            if (serviceInterfaceType == null)
            {
                throw new Exception(
                    $"Error! Could not find service interface type for \"{serviceAssemblyQualifiedName}\", be sure you included the reference (Nuget package) into this project dependencies.");
            }

            return serviceInterfaceType;
        }

        public static Type GetServiceInterfaceTypeByFullName(string serviceFullName)
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.DefinedTypes)
                .Where(t => t.FullName == serviceFullName &&
                            t.ImplementedInterfaces.Any(i => i.Name == "IService"))
                .Select(ti => Type.GetType(ti.AssemblyQualifiedName ?? ""))
                .ToList();

            if (types.Count > 1)
            {
                var typesStr = string.Join(", ", types.Select(t => t.AssemblyQualifiedName));
                throw new Exception(
                    $"Error! Multiple interface for \"{serviceFullName}\" found. \n\n {typesStr}");
            }
            if (types.Count == 0)
            {
                throw new Exception(
                    $"Error! Could not find service interface type for \"{serviceFullName}\", be sure you included the reference (Nuget package) into this project dependencies and you registered the assembly in Program.RegisterAssemblies() method.");
            }

            return types.First();
        }

        #endregion Helpers
    }
}