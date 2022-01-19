using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace .Lease.Api.Infrastructure.FluenScheduler
{
    /// <summary>
    /// Fluensheduler job factory
    /// </summary>
    public class FluentSchedulerJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public FluentSchedulerJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Return Intance using the service provider for dotnet core
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IJob GetJobInstance<T>() where T : IJob
        {
            return _serviceProvider.GetService<T>();
        }
    }
}