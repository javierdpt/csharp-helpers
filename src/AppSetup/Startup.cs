using .RentersInsurance.Api.Infrastructure.AppSetup;
using .RentersInsurance.Api.Services.AppResources;
using .RentersInsurance.Api.Services.RecurrentJobs;
using .ServiceFabric.Infrastructure.Constants;
using .ServiceFabric.Infrastructure.Middlewares;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace .RentersInsurance.Api
{
    /// <summary>
    /// Project Startup class
    /// </summary>
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Startup initialization
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Application service configuration
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAppOptions(Program.Configuration, Program.Settings);

            services.AddAppDatabases();

            services.AddAppDataProtection(
                "RentersInsurance.Api", Program.Configuration.GetSection("AppStorage").Get<StorageOptions>());

            services.AddAppAuth();

            services.AddAppDocs(_hostingEnvironment);

            services.AddCors();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                });

            services.AddAppDependencies();

            services.AddApplicationInsightsTelemetry(Program.Configuration);
            services.AddSnapshotCollector((configuration) => Program.Configuration.Bind(nameof(SnapshotCollectorConfiguration), configuration));
        }

        /// <summary>
        /// Application configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appResourcesService"></param>
        /// <param name="recurrentJobsService"></param>
        public void Configure(
            IApplicationBuilder app,
            IAppResourcesService appResourcesService,
            IRecurrentJobsService recurrentJobsService)
        {
            Program.ServiceProvider = app.ApplicationServices;

            app.UseAppResourcesFlash(appResourcesService); // Register first for performance on returning resources

            app.UseMiddleware<ReverseProxy404HandlerMiddleware>();

            app.UseMiddleware(
                typeof(ExceptionHandlerMiddlewareCustomEnv), Program.AppEnvironment, AppEnvironmentEnum.Production);

            app.UseStaticFiles();

            app.UseAppCors();

            app.UseAuthentication();

            app.UseMvc();

            app.UseAppDocs();

            app.UseRecurrentJobs(recurrentJobsService);
        }
    }
}