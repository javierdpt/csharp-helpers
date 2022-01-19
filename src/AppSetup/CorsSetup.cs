using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RentersInsurance.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Setup application cors
    /// </summary>
    public static class CorsSetup
    {
        /// <summary>
        /// Use app cors
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static void UseAppCors(this IApplicationBuilder app)
        {
            var appOptionsOpt = app
                .ApplicationServices
                .GetService(typeof(IOptions<AppOptions>)) as IOptions<AppOptions>;

            app.UseCors(builder => builder
                .WithOrigins(appOptionsOpt?.Value.AllowedOrigins.Split(','))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            );
        }
    }
}
