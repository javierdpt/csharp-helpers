using System;
using System.Fabric;

namespace HangfireService.Models
{
    public class ServicePartitionDescriptionDto
    {
        public ServicePartitionKind Kind { get; set; }
        public Guid Id { get; set; }
    }
}