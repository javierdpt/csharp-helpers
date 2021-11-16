using FluentScheduler;
using GS.MFH.Lease.Api.Infrastructure.FluenScheduler.Jobs;

namespace GS.MFH.Lease.Api.Infrastructure.FluenScheduler
{
    /// <summary>
    /// LeaseApiBgScheduleJobsRegistry class
    /// </summary>
    public class LeaseApiBgScheduleJobsRegistry : Registry
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public LeaseApiBgScheduleJobsRegistry()
        {
            Schedule<UpdateLeasesPastStatusJob>().ToRunNow().AndEvery(1).Days().At(0, 30);
            Schedule<UpdateLeasesPastStatusJob>().ToRunEvery(1).Days().At(12, 0);

            Schedule<NotifyMovedOutResidentJob>().ToRunNow().AndEvery(1).Days().At(0, 30);
        }
    }
}