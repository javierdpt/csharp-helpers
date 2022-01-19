using Api.Infrastructure.FluenScheduler.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.FluenScheduler
{
    /// <summary>
    /// Class to register dependencies of the bg scheduled jobs
    /// </summary>
    public static class BgScheduleJobsDependencies
    {
        /// <summary>
        /// Configure the servies 
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigureServices(IServiceCollection services)
        {
            #region Services

            services.AddSingleton<UpdateLeasesPastStatusJob>();
            services.AddSingleton<NotifyMovedOutResidentJob>();

            #endregion Services
        }
    }
}