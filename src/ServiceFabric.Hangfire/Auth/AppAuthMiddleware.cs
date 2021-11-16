using HangfireService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireService.Infrastructure.Auth
{
    public class AppAuthMiddleware
    {
        private readonly ConcurrentDictionary<string, DateTime> _tokensCache;
        private readonly RequestDelegate _next;
        private readonly string _issuer;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public AppAuthMiddleware(RequestDelegate next, string issuer, string wellKnownOauthServer)
        {
            _next = next;
            _tokensCache = new ConcurrentDictionary<string, DateTime>();
            _issuer = issuer;
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{_issuer}{wellKnownOauthServer}",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (await AuthenticateAsync(context))
                    await _next(context);
                else
                    await ExecuteChallengeAsync(context);
            }
            catch (UnauthorizedAccessException e)
            {
                await context.GetHandleUnauthorizedResponse(e);
            }
        }

        #region Helpers

        private async Task<bool> AuthenticateAsync(HttpContext context)
        {
            try
            {
                if (context.Request.Cookies.TryGetValue(AccessTokenLabel, out var cookieAccessToken) ||
                    !string.IsNullOrEmpty(cookieAccessToken)
                ) return await ValidateAccessTokenAsync(cookieAccessToken);

                if (!context.Request.HasFormContentType)
                    return false;

                context.Request.EnableRewind();

                var accessToken = string.Empty;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
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
                        respInfo.TryGetValue(AccessTokenLabel, out accessToken);

                    if (string.IsNullOrEmpty(accessToken) && context.Request.Cookies.ContainsKey(AccessTokenLabel))
                        context.Request.Cookies.TryGetValue(AccessTokenLabel, out accessToken);

                    if (respInfo.ContainsKey(ErrorLabel))
                    {
                        var errorMessage = respInfo.ContainsKey(ErrorDescriptionLabel)
                            ? respInfo[ErrorDescriptionLabel].Replace('+', ' ')
                            : "No error message description.";
                        var err = respInfo[ErrorLabel];
                        throw new UnauthorizedAccessException($@"
                        <p style=""text-align: left;"">
                            <strong>Error</strong>: {err} <br>
                            <strong>Error Message</strong>: {errorMessage}
                        </p>
                    ");
                    }
                }

                if (string.IsNullOrWhiteSpace(accessToken))
                    return false;

                if (!context.Request.Cookies.ContainsKey(AccessTokenLabel))
                    context.Response.Cookies.Append(AccessTokenLabel, accessToken);

                return await ValidateAccessTokenAsync(accessToken);
            }
            catch (Exception)
            {
                foreach (var cookie in context.Request.Cookies)
                    context.Response.Cookies.Delete(cookie.Key);
                return false;
            }
        }

        private async Task ExecuteChallengeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.IsAjaxRequest())
                throw new UnauthorizedAccessException();
            await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        private async Task<bool> ValidateAccessTokenAsync(string token)
        {
            if (_tokensCache.TryGetValue(token, out var expiresAtWithBuffer) &&
                expiresAtWithBuffer >= DateTime.Now
            ) return true;

            _tokensCache.TryRemove(token, out _);

            var discoveryDocument = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
            var signingKeys = discoveryDocument.SigningKeys;
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateActor = true,
                ValidateAudience = false
            };

            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParameters, out var securityToken);

            var validToBuffered = securityToken.ValidTo.AddMinutes(-5);
            _tokensCache.AddOrUpdate(
                token,
                validToBuffered,
                (s, time) => validToBuffered);

            return principal.Identity.IsAuthenticated;
        }

        #endregion Helpers

        #region Constants

        private const string AccessTokenLabel = "access_token";
        private const string StateLabel = "state";
        private const string CodeLabel = "code";
        private const string ErrorLabel = "error";
        private const string ErrorDescriptionLabel = "error_description";

        #endregion Constants
    }
}