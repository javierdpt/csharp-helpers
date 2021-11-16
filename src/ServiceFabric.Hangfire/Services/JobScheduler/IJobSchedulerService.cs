using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Reflection;
using System.Threading.Tasks;
using HangfireService.Models;

namespace HangfireService.Services.JobScheduler
{
    public interface IJobSchedulerService
    {
        Task<IEnumerable<ApplicationDescriptionDto>> GetApplicationList(bool includeDetails = true);

        Task<IEnumerable<ServiceDescriptionDto>> GetServiceList(Application app, bool includeDetails = true);

        Task<IEnumerable<ServicePartitionDescriptionDto>> GetServicePartitionList(Uri serviceName);

        Task<IEnumerable<string>> GetServicePartitionEndpoint(Uri serviceName, Partition partition);

        IEnumerable<TypeInfo> GetSupportedTypes();

        IEnumerable<MethodInfo> GetTypeMethods(string typeName);

        IEnumerable<ParameterInfo> GetMethodArguments(string typeName, string methodName);
    }
}