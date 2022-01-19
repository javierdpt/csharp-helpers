using System;
using FluentScheduler;
using Lease.Api.Services.Leases;
using Microsoft.Extensions.Logging;

namespace .Lease.Api.Infrastructure.FluenScheduler.Jobs
{
    /// <summary>
    /// NotifyMovedOutResidentJob IJob class
    /// </summary>
    public class NotifyMovedOutResidentJob : IJob
    {
        private readonly ILogger<NotifyMovedOutResidentJob> _logger;
        private readonly ILeaseUtilsService _leaseUtilsService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="leaseUtilsService"></param>
        public NotifyMovedOutResidentJob(ILogger<NotifyMovedOutResidentJob> logger, ILeaseUtilsService leaseUtilsService)
        {
            _logger = logger;
            _leaseUtilsService = leaseUtilsService;
        }

        /// <summary>
        /// Execute method
        /// </summary>
        public void Execute()
        {
            _logger.LogInformation("Recurrent Job Notify Moved Out Resident.");
            _leaseUtilsService.AddMovedOutResidentCommunityNotificationAsync(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0))).Wait(5 * 60 * 1000);
        }
    }
}