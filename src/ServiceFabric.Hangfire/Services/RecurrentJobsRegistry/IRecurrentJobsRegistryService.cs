namespace HangfireService.Services.RecurrentJobsRegistry
{
    public interface IRecurrentJobsRegistryService
    {
        /// <summary>
        /// Register recurrent Jobs
        /// </summary>
        void RegisterRecurringJobs();
    }
}