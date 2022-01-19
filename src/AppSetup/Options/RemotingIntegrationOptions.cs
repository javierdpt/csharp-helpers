using .ServiceFabric.Infrastructure.ProxyResolver.Dtos;

namespace .RentersInsurance.Api.Infrastructure.Options
{
    /// <summary>
    /// Integrations options
    /// </summary>
    public class RemotingIntegrationOptions
    {
        /// <summary>
        /// Services remoting connection metadata info for comm data service
        /// </summary>
        public RemotingIntegrationServiceOption CommunityDataService { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for jobs service
        /// </summary>
        public RemotingIntegrationServiceOption Jobs { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for caching service
        /// </summary>
        public RemotingIntegrationServiceOption Caching { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for logtrail service
        /// </summary>
        public RemotingIntegrationServiceOption LogTrail { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for places service
        /// </summary>
        public RemotingIntegrationServiceOption AddressService { get; set; }

        /// <summary>
        /// Service remoting connection metadata info for uwq service
        /// </summary>
        public RemotingIntegrationServiceOption UwQuestionsService { get; set; }

        /// <summary>
        /// Service Remoting connection metadata info for EProdQueue Service
        /// </summary>
        public RemotingIntegrationServiceOption EProdService { get; set; }

        /// <summary>
        /// Service remoting connection metadata info for ClickTrack Service
        /// </summary>
        public RemotingIntegrationServiceOption ClickTrackService { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for the messaging service
        /// </summary>
        public RemotingIntegrationServiceOption MessagingService { get; set; }

        /// <summary>
        /// Services remoting connection metadata info for the messaging service
        /// </summary>
        public RemotingIntegrationServiceOption EmailService { get; set; }

        /// <summary>
        /// Service remoting connection metadata info for Policy Service
        /// </summary>
        public RemotingIntegrationServiceOption PolicyService { get; set; }

        /// <summary>
        /// Service remoting connection metadata info for FileNet Service
        /// </summary>
        public RemotingIntegrationServiceOption FileNetService { get; set; }

        /// <summary>
        /// Service remoting connection metadata info for HtmlToPdf Service
        /// </summary>
        public ProxyResolverConfig HtmlToPdfService { get; set; }
    }

    /// <summary>
    /// Remoting connection metadata info
    /// </summary>
    public class RemotingIntegrationServiceOption
    {
        /// <summary>
        /// Base uri
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// PartitionKey
        /// </summary>
        public int PartitionKey { get; set; }

        /// <summary>
        /// ServiceListenerName
        /// </summary>
        public string ServiceListenerName { get; set; }
    }
}