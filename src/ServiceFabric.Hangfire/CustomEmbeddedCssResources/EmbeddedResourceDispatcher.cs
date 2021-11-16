using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace HangfireService.Infrastructure.CustomEmbeddedCssResources
{
    /// <summary>
    /// Alternative to built-in EmbeddedResourceDispatcher, which (for some reasons) is not public.
    /// </summary>
    internal class EmbeddedResourceDispatcher : IDashboardDispatcher
    {
        private readonly Assembly _assembly;
        private readonly string _resourceName;

        public EmbeddedResourceDispatcher(Assembly assembly, string resourceName)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            _assembly = assembly;
            _resourceName = resourceName;
        }

        public Task Dispatch(DashboardContext context)
        {
            return WriteResourceAsync(context.Response, _assembly, _resourceName);
        }

        private static async Task WriteResourceAsync(DashboardResponse response, Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException($@"Resource '{resourceName}' not found in assembly {assembly}.");

                string currStr;
                using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                    currStr = reader.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(currStr))
                    await response.WriteAsync(currStr);
            }
        }
    }
}