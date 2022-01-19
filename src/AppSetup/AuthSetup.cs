using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using Okta.AspNet.Abstractions;
using Okta.AspNetCore;

namespace .RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// Auth setup
    /// </summary>
    public static class AuthSetup
    {
        /// <summary>
        /// Add App auth
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppAuth(this IServiceCollection services)
        {
            var secKey = new SymmetricSecurityKey(
                Convert.FromBase64String(Program.Configuration[".ServiceFabric.Settings:SecKeyString"]));
            services
                .AddAuthentication()
                .AddJwtBearer(AppSchemeName, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = secKey,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                })
                .AddJwtBearer(OktaSchemeName, opts =>
                {
                    opts.Authority = Program.Configuration[OktaAuthority];
                    opts.Audience = Program.Configuration[OktaAudience];
                    opts.BackchannelHttpHandler = new UserAgentHandler("okta-aspnetcore", 
                                                        typeof(OktaAuthenticationOptionsExtensions).Assembly.GetName().Version);
                })
            ;

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    // Add multiple auth schemes
                    .AddAuthenticationSchemes(AppSchemeName, OktaSchemeName)
                    .Build();
            });

            return services;
        }

        #region Constants

        private const string AppSchemeName = "RentersInsuranceApiSchemeName";
        private const string OktaAuthority = "Okta:Authority";
        private const string OktaAudience = "Okta:Audience";
        private const string OktaSchemeName = "OktaScheme";

        #endregion Constants
    }
}