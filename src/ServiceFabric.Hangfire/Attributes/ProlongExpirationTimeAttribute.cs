using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;

namespace HangfireService.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private readonly TimeSpan _jobExpirationTimeout;

        public ProlongExpirationTimeAttribute(int days = 1)
        {
            _jobExpirationTimeout = TimeSpan.FromDays(days);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = _jobExpirationTimeout;
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
        }
    }
}