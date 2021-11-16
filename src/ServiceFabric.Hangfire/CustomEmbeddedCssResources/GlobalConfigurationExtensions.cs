using System;
using System.Reflection;
using Hangfire;
using Hangfire.Dashboard;

namespace HangfireService.Infrastructure.CustomEmbeddedCssResources
{
    public static class GlobalConfigurationExtensions
    {
        /// <summary>
        /// Configures Hangfire to use the dark dashboard theme
        /// </summary>
        /// <param name="configuration">Global configuration</param>
        /// <param name="resourceNames"></param>
        public static IGlobalConfiguration AddCustomEmbeddedCssResources(
            this IGlobalConfiguration configuration,
            params string[] resourceNames)
        {
            var assembly = typeof(GlobalConfigurationExtensions).GetTypeInfo().Assembly;

            // Check if the resources where embedded as Resources
            foreach (var resourceName in resourceNames)
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null) continue;
                    throw new ArgumentException($@"Resource '{resourceName}' not found in assembly {assembly}.");
                }
            }

            // register dispatchers for CSS
            var dispatchers = new CompositeDispatcher(DashboardRoutes.Routes.FindDispatcher("/css000").Item1);
            foreach (var resourceName in resourceNames)
            {
                dispatchers.AddDispatcher(new EmbeddedResourceDispatcher(assembly, resourceName));
            }

            DashboardRoutes.Routes.Replace("/css[0-9]+", dispatchers);

            return configuration;
        }
    }
}