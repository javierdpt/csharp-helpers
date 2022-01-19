using ServiceFabric.Infrastructure.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Diagnostics;
using System.Fabric;
using System.IO;
using System.Linq;

namespace RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Swagger setup
    /// </summary>
    public static class SwaggerSetup
    {
        /// <summary>
        /// Add app swagger doc
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hostingEnvironment"></param>
        public static IServiceCollection AddAppDocs(
            this IServiceCollection services,
            IHostingEnvironment hostingEnvironment)
        {
            if (Program.AppEnvironment != AppEnvironmentEnum.Production)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info
                    {
                        Version = "v1",
                        Title = "RentersInsurance API",
                        Description = "RentersInsurance Service",
                        TermsOfService = "None",
                        Contact = new Contact
                        {
                            Name = "Multi Family Housing PMSWebIT Team",
                            Email = "pmswebitdevs@assurant.com"
                        },
                        License = new License { Name = "Assurant Inc.", Url = "https://www.assurant.com/" }
                    });

                    c.IncludeXmlComments(
                        new DirectoryInfo(hostingEnvironment.ContentRootPath)
                            .GetFiles($"{Process.GetCurrentProcess().ProcessName}.xml", SearchOption.AllDirectories)
                            .First()
                            .FullName
                    );
                });
            }

            return services;
        }

        /// <summary>
        /// Use swagger docs
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static void UseAppDocs(this IApplicationBuilder app)
        {
            if (Program.AppEnvironment != AppEnvironmentEnum.Production)
            {
                var statelessServiceContext = app
                    .ApplicationServices
                    .GetService<StatelessServiceContext>();

                var basePath = $"{statelessServiceContext.ServiceName.AbsolutePath}";
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "Docs/Api/{documentName}/swagger.json";
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.BasePath = basePath);
                });
                app.UsePathBase(basePath);
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("Api/v1/swagger.json", "Api V1");
                    c.RoutePrefix = "Docs";
                    c.DocExpansion(DocExpansion.None);
                });
            }
        }
    }
}