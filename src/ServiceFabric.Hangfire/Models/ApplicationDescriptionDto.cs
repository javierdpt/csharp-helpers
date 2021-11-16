using System.Collections.Generic;

namespace HangfireService.Models
{
    public class ApplicationDescriptionDto
    {
        public string AbsolutePath { get; set; }
        public string AbsoluteUri { get; set; }
        public IEnumerable<ServiceDescriptionDto> Services { get; set; }
    }
}