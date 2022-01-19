namespace .RentersInsurance.Api.Infrastructure.Options
{
    /// <summary>
    /// Db options
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// MultifamilyHousing connection string
        /// </summary>
        public string MultifamilyHousing { get; set; }

        /// <summary>
        /// MultifamilyHousingAnalytics connection string
        /// </summary>
        public string MultifamilyHousingAnalytics { get; set; }

        /// <summary>
        /// RentersServicesDb connection string
        /// </summary>
        public string RentersServicesDb { get; set; }
    }
}