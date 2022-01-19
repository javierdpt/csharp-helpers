using RentersInsurance.Api.Services.RecurrentJobs;
using Microsoft.AspNetCore.Builder;

namespace .RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Swagger setup
    /// </summary>
    public static class RecurrentJobsSetup
    {
        /// <summary>
        /// Setup recurrent jobs
        /// </summary>
        /// <param name="app"></param>
        /// <param name="recurrentJobsService"></param>
        /// <returns></returns>
        public static void UseRecurrentJobs(this IApplicationBuilder app, IRecurrentJobsService recurrentJobsService)
        {
            recurrentJobsService.RegisterRecurringJobs();
        }
    }
}