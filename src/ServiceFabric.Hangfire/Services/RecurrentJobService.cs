using HangfireService.Infrastructure.JobExecutionHelper;
using HangfireService.Interfaces;
using Hangfire;
using NLog;
using System;
using System.Threading.Tasks;

namespace HangfireService.Services
{
    public class RecurrentJobService : IRecurrentJobService
    {
        private readonly ILogger _logger;

        public RecurrentJobService()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task AddOrUpdate_Remoting(
            string name,
            string cron,
            string queue,
            TimeZoneInfo timezone,
            string uri,
            string listenerName,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters)
        {
            await AddOrUpdate_Partitioned(name,
                cron,
                queue,
                timezone,
                uri,
                listenerName,
                null,
                serviceAssemblyQualifiedName,
                methodName,
                parameters);
        }

        public async Task AddOrUpdate_Partitioned(
            string name,
            string cron,
            string queue,
            TimeZoneInfo timezone,
            string uri,
            string listenerName,
            object partitionKey,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters)
        {
            try
            {
                RecurringJob.AddOrUpdate(
                    name,
                    () => JobsExecutionHelper.ExecuteRemoting(
                        uri,
                        serviceAssemblyQualifiedName,
                        listenerName,
                        partitionKey,
                        methodName,
                        parameters),
                    cron,
                    timezone,
                    queue
                );
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Recurring AddOrUpdate using AssemblyQualifiedName (Error: {e.Message}).");
                throw;
            }
        }

        public async Task RemoveIfExist(string recurrentJobId)
        {
            try
            {
                RecurringJob.RemoveIfExists(recurrentJobId);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Recurring RemoveIfExist (Error: {e.Message}).");
                throw;
            }
        }

        public async Task Trigger(string recurrentJobId)
        {
            try
            {
                RecurringJob.Trigger(recurrentJobId);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Recurring Trigger (Error: {e.Message}).");
                throw;
            }
        }
    }
}