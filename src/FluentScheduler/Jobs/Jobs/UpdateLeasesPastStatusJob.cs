using FluentScheduler;
using GS.MFH.Lease.Api.Services.Leases;
using Microsoft.Extensions.Logging;
using NLog;

namespace GS.MFH.Lease.Api.Infrastructure.FluenScheduler.Jobs
{
    /// <summary>
    /// UpdateLeasesPastStatusJob IJob class
    /// </summary>
    public class UpdateLeasesPastStatusJob : IJob
    {
        private readonly ILogger<UpdateLeasesPastStatusJob> _logger;
        private readonly ILeaseUtilsService _leaseUtilsService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="leaseUtilsService"></param>
        public UpdateLeasesPastStatusJob(ILogger<UpdateLeasesPastStatusJob> logger, ILeaseUtilsService leaseUtilsService)
        {
            _logger = logger;
            _leaseUtilsService = leaseUtilsService;
        }

        /// <summary>
        /// Execute method
        /// </summary>
        public void Execute()
        {
            _logger.LogInformation("Running recurrent job Update Leases Past Status.");
            _leaseUtilsService.UpdateLeasesPastStatusAsync().Wait(5 * 60 * 1000);
        }
    }
}