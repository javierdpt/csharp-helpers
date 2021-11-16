using GS.MFH.ServiceFabric.Infrastructure.Constants;
using GS.MFH.ServiceFabric.Infrastructure.Extensions;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.ServiceFabric;
using Microsoft.ApplicationInsights.ServiceFabric.Module;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Actors.Runtime;
using NLog;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;

namespace GS.MFH.Lease.LoadDataActor
{
    internal static class Program
    {
        public static IConfigurationRoot Configurations;
        public static IDictionary<string, string> Settings;
        public static AppEnvironmentEnum AppEnvironment;

        private static void Main()
        {
            try
            {
                ActorRuntime.RegisterActorAsync<LoadDataActor>((context, actorType) =>
                {
                    Init(context);
                    InitTelemetry(context);

                    return new ActorService(context, actorType);
                }).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }

        #region Helpers

        private static void Init(StatefulServiceContext context)
        {
            Settings = context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                .ToDictionary()["GS.MFH.ServiceFabric.Settings"];

            Configurations = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Settings["Environment"]}.json")
                .Build();

            Enum.TryParse(Settings["Environment"], out AppEnvironment);

            LogManager.LoadConfiguration("NLog.config");
            LogManager.Configuration.Variables["connectionString"] =
                string.Format(
                    Configurations.GetConnectionString("MultifamilyHousingAnalytics"),
                    Settings["MfhDbUserName"],
                    Settings["MfhDbPassword"]);
            LogManager.Configuration.Variables["configDir"] = Configurations["Logging:LogFolder"];
            LogManager.Configuration.Variables["appEnvironment"] = Settings["Environment"];
            LogManager.Configuration.Variables["dbLogMinLevel"] = Configurations["Logging:DbLogMinLevel"];
            LogManager.GetCurrentClassLogger().Info($"{nameof(LoadDataActor)} service started.");
        }

        private static void InitTelemetry(ServiceContext context)
        {
            var instrumentationKey = Configurations["ApplicationInsights:InstrumentationKey"];
            var config = TelemetryConfiguration.CreateDefault();
            config.TelemetryInitializers.Add(
                FabricTelemetryInitializerExtension.CreateFabricTelemetryInitializer(context)
            );
            config.InstrumentationKey = instrumentationKey;
            config.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            config.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
            new DependencyTrackingTelemetryModule().Initialize(config);
            new ServiceRemotingRequestTrackingTelemetryModule().Initialize(config);
            new ServiceRemotingDependencyTrackingTelemetryModule().Initialize(config);
            new QuickPulseTelemetryModule().Initialize(config);
        }

        #endregion Helpers
    }
}