using System.Collections.Generic;

namespace HangfireService.Models
{
    public class RecurrentJob
    {
        public string Id { get; set; }

        public string CronTab { get; set; }

        public string QueueName { get; set; }

        public string TimeZoneId { get; set; }

        public ServiceInfo ServiceInfo { get; set; }

        public Dictionary<string, object> Params { get; set; }

        public RecurrentJobDateParam ParamDate { get; set; } = null;

    }
}