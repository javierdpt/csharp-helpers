using System.Collections.Generic;

namespace HangfireService.Models
{
    public class DynamicRecurringJobsConfig
    {
        public List<RecurrentJob> Jobs { get; set; }
    }
}