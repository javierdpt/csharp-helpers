using HangfireService.Infrastructure.JobExecutionHelper;
using HangfireService.Models;
using HangfireService.Services.ProxyResolver;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireService.Services.RecurrentJobsRegistry
{
    /// <summary>
    /// RecurrentJobsRegistry class
    /// </summary>
    public class RecurrentJobsRegistryService : IRecurrentJobsRegistryService
    {
        private DateTime _configRead;
        public TimeZoneInfo EasterTimeUs { get; } = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        /// <summary>
        /// Ctor
        /// </summary>
        public RecurrentJobsRegistryService()
        {
            _configRead = DateTime.MinValue;
        }

        /// <inheritdoc />
        /// <summary>
        /// Register recurring Jobs
        /// </summary>
        public void RegisterRecurringJobs()
        {
            RecurringJob.AddOrUpdate(
                "HangfireService - Set RecurringJobs",
                () => SetupDynamicsRecurringJobs(),
                Cron.Daily(0, 5),
                EasterTimeUs
            );
        }

        public async Task SetupDynamicsRecurringJobs()
        {
            using (var scope = Startup.ServiceProvider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<RecurrentJobsRegistryService>>();
                try
                {
                    var proxyResolver = scope.ServiceProvider.GetService<IProxyResolverService>();

                    var config = await proxyResolver
                        .ConfigManagerService()
                        .LoadConfigsAsync(
                            ConfigAppName,
                            ConfigName
                        );

                    if (config == null)
                    {
                        throw new Exception($"Could not find configuration for app \"{ConfigAppName}\" configName \"{ConfigName}\".");
                    }
                    if (_configRead == config.LastModifiedOnUtc)
                    {
                        return;
                    }

                    _configRead = config.LastModifiedOnUtc;
                    var jobs = config.Data.ToObject<DynamicRecurringJobsConfig>().Jobs;

                    var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
                    foreach (var recurrentJob in
                        recurringJobs.Where(rj =>
                            rj.Id.Contains("(Dyn)") &&
                            jobs.FirstOrDefault(j => j.Id == rj.Id) == null
                        )
                    )
                    {
                        RecurringJob.RemoveIfExists(recurrentJob.Id);
                    }

                    foreach (var jobConfig in jobs)
                    {
                        if (!string.IsNullOrEmpty(jobConfig.ParamDate?.Format))
                        {
                            var parameters = jobConfig.Params.Values.ToArray();
                            var dayOfMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month) < jobConfig.ParamDate.DayOfMonth
                                ? DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)
                                : jobConfig.ParamDate.DayOfMonth;
                            parameters[jobConfig.ParamDate.Order] = DateTime
                                                                        .Now
                                                                        .AddMonths(jobConfig.ParamDate.AddMonths)
                                                                        .AddDays(dayOfMonth <= 0 ? 0 : -(DateTime.Now.Day - dayOfMonth))
                                                                        .ToString(jobConfig.ParamDate.Format);

                            RecurringJob.AddOrUpdate(
                                jobConfig.Id,
                                () => JobsExecutionHelper.EnqueueRemoting(
                                    jobConfig.ServiceInfo.Uri,
                                    jobConfig.ServiceInfo.ServiceAssemblyQualifiedName,
                                    jobConfig.ServiceInfo.ListenerName,
                                    jobConfig.ServiceInfo.MethodName,
                                    parameters
                                ),
                                jobConfig.CronTab,
                                string.IsNullOrEmpty(jobConfig.TimeZoneId)
                                    ? EasterTimeUs
                                    : TimeZoneInfo.FindSystemTimeZoneById(jobConfig.TimeZoneId),
                                jobConfig.QueueName);
                            continue;
                        }

                        RecurringJob.AddOrUpdate(
                            jobConfig.Id,
                            () => JobsExecutionHelper.ExecuteRemoting(
                                jobConfig.ServiceInfo.Uri,
                                jobConfig.ServiceInfo.ServiceAssemblyQualifiedName,
                                jobConfig.ServiceInfo.ListenerName,
                                null,
                                jobConfig.ServiceInfo.MethodName,
                                jobConfig.Params.Values.ToArray()
                            ),
                            jobConfig.CronTab,
                            string.IsNullOrEmpty(jobConfig.TimeZoneId)
                                ? EasterTimeUs
                                : TimeZoneInfo.FindSystemTimeZoneById(jobConfig.TimeZoneId),
                            jobConfig.QueueName);
                    }
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, $"Error loading dynamic Jobs from ConfigManager AppName: \"{ConfigAppName}\", ConfigName: \"{ConfigName}\".");
                    throw;
                }
            }
        }

        #region Constants

        private const string ConfigAppName = "Jobs.HangfireServices";
        private const string ConfigName = "DynamicsRecurringJobs";

        #endregion Constants
    }
}