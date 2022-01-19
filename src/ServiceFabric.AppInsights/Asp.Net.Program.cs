using ServiceFabric.Infrastructure.Constants;
using ServiceFabric.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Runtime;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Threading;

namespace Api
{
    internal static class Program
    {
        /// <summary>
        /// App environment read from Configurations
        /// </summary>
        public static AppEnvironmentEnum AppEnvironment;

        /// <summary>
        /// App Configurations (read from ServiceFabric)
        /// </summary>
        public static IDictionary<string, string> Settings;

        /// <summary>
        /// App configurations (read from appsettings.json)
        /// </summary>
        public static IConfigurationRoot Configurations { get; set; }

        private static void Main()
        {
            try
            {
                ServiceRuntime.RegisterServiceAsync("LeaseApiType", context =>
                {
                    Init(context);
                    return new LeaseApi(context);
                }).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(LeaseApi).Name);

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static void Init(StatelessServiceContext context)
        {
            // Init Configurations
            Settings = context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                .ToDictionary()[".ServiceFabric.Settings"];
            Configurations = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Settings["Environment"]}.json")
                .Build();
            Enum.TryParse(Settings["Environment"], out AppEnvironment);

            // Init NLog
            LogManager.LoadConfiguration("NLog.config");
            LogManager.Configuration.Variables["connectionString"] =
                string.Format(
                    Configurations.GetConnectionString("MultifamilyHousingAnalytics"),
                    Settings["MfhDbUserName"],
                    Settings["MfhDbPassword"]);
            LogManager.Configuration.Variables["configDir"] = Configurations["Logging:LogFolder"];
            LogManager.Configuration.Variables["appEnvironment"] = Settings["Environment"];
            LogManager.GetCurrentClassLogger().Info($"{nameof(LeaseApi)} service started.");
        }
    }
}