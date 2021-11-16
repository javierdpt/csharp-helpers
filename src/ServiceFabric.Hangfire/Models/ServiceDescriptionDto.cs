using System.Collections.Generic;

namespace HangfireService.Models
{
    public class ServiceDescriptionDto
    {
        public string AbsoluteUri { get; set; }
        public string AbsolutePath { get; set; }
        public string NamespaceHint { get; set; }
        public IEnumerable<string> Endpoints { get; set; }
        public IEnumerable<ServicePartitionDescriptionDto> PartitionList { get; set; }
        public IEnumerable<TypeDescriptionDto> SupportedTypes { get; set; }
    }
}