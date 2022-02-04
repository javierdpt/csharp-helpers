using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Core
{
    public static class CosmosServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmos(this IServiceCollection services)
        {
            services
                .AddSingleton(CreateCosmosClient)
                .AddSingleton<ICosmosLinqQuery, CosmosLinkQuery>()
                .AddSingleton<IEntityRepository>(provider => CreateRepository<EntityRepository>(provider, "EntityContainer"))
            ;
            return services;
        }

        private static CosmosClient CreateCosmosClient(IServiceProvider provider)
        {
            var config = provider.GetRequiredService<IConfiguration>();

            var serializerSettings = CosmosDbJsonSerializerSettings.Default;
            CosmosSerializationSettings.JsonSerializerSettingsAction.Invoke(serializerSettings);

            var builder = new CosmosClientBuilder(config[ConfigConstants.CosmosConnectionString])
                .WithCustomSerializer(new MicrosoftCosmosJsonDotNetSerializer(serializerSettings));

            if (config["ConnectionStrings:ConnectionMode"] == "Gateway")
            {
                builder = builder.WithConnectionModeGateway();
            }

            return builder.Build();
        }

        private static T CreateRepository<T>(IServiceProvider provider, string containerName) where T : class
        {
            var config = provider.GetRequiredService<IConfiguration>();

            return (T)Activator.CreateInstance(typeof(T),
                provider.GetRequiredService<CosmosClient>().GetContainer(
                    config[ConfigConstants.CosmosDatabaseName],
                    containerName),
                provider.GetRequiredService<ICosmosLinqQuery>());
        }
    }

    public class CosmosDbJsonSerializerSettings : JsonSerializerSettings
    {
        private CosmosDbJsonSerializerSettings()
        {
            this.Formatting = Formatting.Indented;
            this.TypeNameHandling = TypeNameHandling.None;
            this.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            this.ContractResolver = (IContractResolver) new CamelCasePropertyNamesContractResolver();
            this.Converters.Add((JsonConverter) new StringEnumConverter());
        }

        public CosmosDbJsonSerializerSettings Copy() => (CosmosDbJsonSerializerSettings) this.MemberwiseClone();

        public static CosmosDbJsonSerializerSettings Default => new CosmosDbJsonSerializerSettings();
    }
}
