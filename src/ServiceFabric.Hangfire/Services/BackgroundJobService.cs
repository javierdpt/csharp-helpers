using Common.Extensions;
using HangfireService.Infrastructure.Extensions;
using HangfireService.Infrastructure.JobExecutionHelper;
using HangfireService.Interfaces;
using HangfireService.Services.HangfireBackgroundJobClient;
using Hangfire;
using NLog;
using System;
using System.Threading.Tasks;

namespace HangfireService.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IHangfireBackgroundJobClient _backgroundJobClient;
        private readonly ILogger _logger;

        public BackgroundJobService(IHangfireBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public Task<string> Enqueue_Remoting(
            string uri,
            string listenerName,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters)
        {
            try
            {
                return Enqueue_Partitioned(uri,
                                         listenerName,
                                         null,
                                         serviceAssemblyQualifiedName,
                                         methodName,
                                         parameters);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Enqueue Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Enqueue_Remoting_V2(
            string uri,
            string listenerName,
            string serviceFullName,
            string methodName,
            object[] parameters,
            QueueNamesEnum queue)
        {
            try
            {
                return Enqueue_Partitioned_V2(
                    uri,
                    listenerName,
                    null,
                    serviceFullName,
                    methodName,
                    parameters,
                    queue);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Enqueue Remoting V2 using FullName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Enqueue_Partitioned(
            string uri,
            string listenerName,
            object partitionKey,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters)
        {
            try
            {
                var jobId = _backgroundJobClient.Enqueue(() =>
                    JobsExecutionHelper.ExecuteRemoting(uri,
                        serviceAssemblyQualifiedName,
                        listenerName,
                        partitionKey,
                        methodName,
                        parameters)
                );
                return Task.FromResult(jobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Enqueue Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Enqueue_Partitioned_V2(
            string uri,
            string listenerName,
            object partitionKey,
            string serviceFullName,
            string methodName,
            object[] parameters,
            QueueNamesEnum queue)
        {
            try
            {
                var jobId = _backgroundJobClient.Enqueue(() =>
                    JobsExecutionHelper.ExecuteRemotingV2(
                        uri,
                        serviceFullName,
                        listenerName,
                        partitionKey,
                        methodName,
                        parameters
                    ),
                    queue
                );
                return Task.FromResult(jobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Enqueue Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> ContinueWith_Remoting(
            string jobId,
            string uri,
            string listenerName,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters)
        {
            try
            {
                var nextJobId = _backgroundJobClient.ContinueWith(
                    jobId,
                    () => JobsExecutionHelper.ExecuteRemoting(
                        uri,
                        serviceAssemblyQualifiedName,
                        listenerName,
                        null,
                        methodName,
                        parameters)
                );
                return Task.FromResult(nextJobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing ContinueWith Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> ContinueWith_Remoting_V2(
            string uri,
            string listenerName,
            string serviceFullName,
            string methodName,
            object[] parameters,
            string jobId,
            QueueNamesEnum queue)
        {
            try
            {
                var nextJobId = _backgroundJobClient.ContinueWith(
                    jobId,
                    () => JobsExecutionHelper.ExecuteRemotingV2(
                        uri,
                        serviceFullName,
                        listenerName,
                        null,
                        methodName,
                        parameters),
                    queue
                );
                return Task.FromResult(nextJobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing ContinueWith Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Schedule_Delayed_Remoting(
            string uri,
            string listenerName,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters,
            TimeSpan delay)
        {
            try
            {
                var jobId = BackgroundJob.Schedule(
                    () => JobsExecutionHelper.ExecuteRemoting(
                        uri,
                        serviceAssemblyQualifiedName,
                        listenerName,
                        null,
                        methodName,
                        parameters),
                    delay
                );
                return Task.FromResult(jobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Schedule Delayed Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Schedule_EnqueueAt_Remoting(
            string uri,
            string listenerName,
            string serviceAssemblyQualifiedName,
            string methodName,
            object[] parameters,
            DateTime enqueueAt,
            TimeZoneInfo timeZone)
        {
            try
            {
                var jobId = BackgroundJob.Schedule(
                    () => JobsExecutionHelper.ExecuteRemoting(
                        uri,
                        serviceAssemblyQualifiedName,
                        listenerName,
                        null,
                        methodName,
                        parameters),
                    enqueueAt.GetDateTimeOffsetForTimeZone(timeZone)
                );
                return Task.FromResult(jobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Schedule EnqueueAt Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<string> Schedule_EnqueueAt_Remoting_V2(
            string uri,
            string listenerName,
            string serviceFullName,
            string methodName,
            object[] parameters,
            DateTime enqueueAt,
            TimeZoneInfo timeZone)
        {
            try
            {
                var jobId = BackgroundJob.Schedule(
                    () => JobsExecutionHelper.ExecuteRemotingV2(
                        uri,
                        serviceFullName,
                        listenerName,
                        null,
                        methodName,
                        parameters),
                    enqueueAt.GetDateTimeOffsetForTimeZone(timeZone)
                );
                return Task.FromResult(jobId);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Schedule EnqueueAt Remoting using AssemblyQualifiedName (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<bool> Requeue(string jobId)
        {
            try
            {
                return Task.FromResult(BackgroundJob.Requeue(jobId));
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Requeue (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }

        public Task<bool> Delete(string jobId)
        {
            try
            {
                return Task.FromResult(BackgroundJob.Delete(jobId));
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Fatal! Executing Delete (Error: {e.Message}).");
                throw e.ToFriendlySerializerException();
            }
        }
    }
}