using System.Fabric;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;

namespace .RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// App data protection setup
    /// </summary>
    public static class DataProtectionSetup
    {
        /// <summary>
        /// Add app data protection
        /// </summary>
        /// <param name="services"></param>
        /// <param name="appName"></param>
        /// <param name="storageOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppDataProtection(
            this IServiceCollection services,
            string appName,
            StorageOptions storageOptions)
        {
            var statelessServiceContext = services
                .BuildServiceProvider()
                .GetService<StatelessServiceContext>();

            var storageAccount = CloudStorageAccount.Parse(storageOptions.ConnectionString);
            var blobName = $"{statelessServiceContext.CodePackageActivationContext.ApplicationName.Replace("fabric:/", string.Empty).Replace("/", string.Empty)}-{Program.AppEnvironment}.xml";
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(storageOptions.AuthKeysContainerName);
            services.AddDataProtection()
                .SetApplicationName(appName)
                .PersistKeysToAzureBlobStorage(container, blobName)
            //  .ProtectKeysWithAzureKeyVault("KeyIdentifier", "ClientId", "ClientSecret")
            ;

            return services;
        }
    }

    /// <summary>
    /// Storage options
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Storage connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// AuthKeys blob container name
        /// </summary>
        public string AuthKeysContainerName { get; set; }
    }
}