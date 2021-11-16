using HangfireService.Infrastructure;
using HangfireService.Interfaces;
using Hangfire;
using Hangfire.States;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

// ReSharper disable IdentifierTypo

namespace HangfireService.Services.HangfireBackgroundJobClient
{
    public class HangfireBackgroundJobClient : IHangfireBackgroundJobClient
    {
        private BackgroundJobClient _bgJobClient;

        public string Enqueue(
            Expression<Func<Task>> operation,
            QueueNamesEnum queue = QueueNamesEnum.@default)
        {
            if (queue == QueueNamesEnum.@default)
            {
                return BackgroundJob.Enqueue(operation);
            }

            CreateBgClient();
            return _bgJobClient.Create(operation, new EnqueuedState(queue.ToString()));
        }

        public string ContinueWith(
            string jobId, Expression<Func<Task>> operation,
            QueueNamesEnum queue = QueueNamesEnum.@default)
        {
            if (queue == QueueNamesEnum.@default)
            {
                return BackgroundJob.ContinueWith(jobId, operation);
            }

            CreateBgClient();
            return _bgJobClient.ContinueWith(jobId, operation, new EnqueuedState(queue.ToString()));
        }

        #region Hepers

        private void CreateBgClient()
        {
            if (_bgJobClient == null)
            {
                _bgJobClient = new BackgroundJobClient();
            }
        }

        #endregion Hepers
    }
}