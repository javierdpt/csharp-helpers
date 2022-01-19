using System.Threading.Tasks;
namespace .Lease.Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// Task extension methods
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Fire and forgets a task
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task)
        {
            Task.Run(async () => await task).ConfigureAwait(false);
        }
    }
}