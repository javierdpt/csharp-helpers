using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HangfireService.Infrastructure.Extensions;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NLog;

namespace HangfireService.Infrastructure.Auth
{
    /// <summary>
    /// HangfireAuthorizationFilter class
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly ILogger _logger;
        private const string AccessTokenLabel = "access_token";
        private const string StateLabel = "state";
        private const string CodeLabel = "code";
        private const string ErrorLabel = "error";
        private const string ErrorDescriptionLabel = "error_description";

        public HangfireAuthorizationFilter()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Authorize class
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            try
            {
                if (httpContext.User.Identity.IsAuthenticated)
                    return true;

                var issuer = Startup.Configuration["Okta:Authority"];
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{issuer}{Startup.Configuration["Okta:WellknownOauthServer"]}",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());

                if (!AuthenticateUser(httpContext, issuer, configurationManager))
                {
                    ExecuteChallenge(httpContext);
                    return false;
                }

                return true;
            }
            catch (SecurityTokenExpiredException e)
            {
                _logger.Fatal(e, $"Something bad happened while running {GetType().Name}. (Challenging)");
                httpContext.SignOutAsync().GetAwaiter().GetResult();
                httpContext.Response.Cookies.Delete(AccessTokenLabel);
                ExecuteChallenge(httpContext);
                return false;
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Something bad happened while running {GetType().Name}.");
                throw;
            }
        }

        private static void ExecuteChallenge(HttpContext httpContext)
        {
            if (!httpContext.Request.IsAjaxRequest())
            {
                httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        private static bool AuthenticateUser(
            HttpContext httpContext,
            string issuer,
            ConfigurationManager<OpenIdConnectConfiguration> configurationManager
        )
        {
            if (httpContext.Request.Cookies.ContainsKey(AccessTokenLabel) &&
                httpContext.Request.Cookies.TryGetValue(AccessTokenLabel, out var cookieAccessToken) &&
                !string.IsNullOrEmpty(cookieAccessToken)
            )
            {
                var claimsFromCookieAccessToken = GetUserPrincipal(httpContext, issuer, configurationManager, cookieAccessToken);
                return claimsFromCookieAccessToken.Identity.IsAuthenticated;
            }

            if (!httpContext.Request.HasFormContentType)
                return false;

            httpContext.Request.EnableRewind();

            var accessToken = string.Empty;
            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
            {
                var body = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(body))
                    return false;

                var respInfo = body.Split('&')
                    .Select(x => x.Split('='))
                    .GroupBy(g => g[0])
                    .Select(x => x.First())
                    .ToDictionary(x => x[0], x => x[1]);
                if (respInfo.ContainsKey(StateLabel) && respInfo.ContainsKey(CodeLabel) && respInfo.ContainsKey(AccessTokenLabel))
                {
                    respInfo.TryGetValue(AccessTokenLabel, out accessToken);
                }

                if (string.IsNullOrEmpty(accessToken) && httpContext.Request.Cookies.ContainsKey(AccessTokenLabel))
                {
                    httpContext.Request.Cookies.TryGetValue(AccessTokenLabel, out accessToken);
                }

                if (respInfo.ContainsKey(ErrorLabel))
                {
                    var errmsg = respInfo.ContainsKey(ErrorDescriptionLabel)
                        ? respInfo[ErrorDescriptionLabel].Replace('+', ' ')
                        : "No error message description.";
                    var err = respInfo[ErrorLabel];
                    throw new UnauthorizedAccessException($@"
                        <p style=""text-align: left;"">
                            <strong>Error</strong>: {err} <br>
                            <strong>Error Message</strong>: {errmsg}
                        </p>
                    ");
                }
            }

            if (string.IsNullOrWhiteSpace(accessToken))
                return false;

            if (!httpContext.Request.Cookies.ContainsKey(AccessTokenLabel))
            {
                httpContext.Response.Cookies.Append(AccessTokenLabel, accessToken);
            }

            var claims = GetUserPrincipal(httpContext, issuer, configurationManager, accessToken);
            return claims.Identity.IsAuthenticated;
        }

        private static ClaimsPrincipal GetUserPrincipal(HttpContext httpContext, string issuer, ConfigurationManager<OpenIdConnectConfiguration> configurationManager, string accessToken)
        {
            var claims = ValidateAccessToken(accessToken, issuer, configurationManager)
                            .GetAwaiter()
                            .GetResult();

            httpContext.SignOutAsync().GetAwaiter().GetResult();
            httpContext.SignInAsync("Cookies", claims).GetAwaiter().GetResult();
            return claims;
        }

        private static async Task<ClaimsPrincipal> ValidateAccessToken(
            string token,
            string issuer,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
            CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(issuer)) throw new ArgumentNullException(nameof(issuer));

            var discoveryDocument = await configurationManager.GetConfigurationAsync(ct);
            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateActor = true,
                ValidateAudience = false,

                //ValidateIssuerSigningKey = true
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
    }
}