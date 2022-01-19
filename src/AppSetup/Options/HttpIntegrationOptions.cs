namespace .RentersInsurance.Api.Infrastructure.Options
{
    /// <summary>
    /// Http integration options
    /// </summary>
    public class HttpIntegrationOptions
    {
        /// <summary>
        /// Resource app url
        /// </summary>
        public string ResourcesAppUrl { get; set; }

        /// <summary>
        /// Spectrum api url
        /// </summary>
        public string SpectrumApiUrl { get; set; }

        /// <summary>
        /// Moratorium Service url
        /// </summary>
        public string MoratoriumServiceUrl { get; set; }

        /// <summary>
        /// Billing Service url
        /// </summary>
        public string BillingServiceUrl { get; set; }

        /// <summary>
        /// Html2Pdf Service url
        /// </summary>>
        public string Html2PdfWebServiceUrl { get; set; }
    }
}