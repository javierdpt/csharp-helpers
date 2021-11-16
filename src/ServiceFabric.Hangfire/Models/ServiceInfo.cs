namespace HangfireService.Models
{
    public class ServiceInfo
    {
        public string Uri { get; set; }

        public string ListenerName { get; set; }

        public string ServiceAssemblyQualifiedName { get; set; }

        public string MethodName { get; set; }
    }
}