using .RentersInsurance.Api.Infrastructure.Constants;
using .RentersInsurance.Api.Services.AppResources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using AppEndpointsConst = .RentersInsurance.Api.Infrastructure.Constants.AppEndpointsConst;

namespace .RentersInsurance.Api.Infrastructure.AppSetup
{
    /// <summary>
    /// App Resources Flash map middleware Setup
    /// </summary>
    public static class AppResourcesFlashSetup
    {
        /// <summary>
        /// Use app resource Flash
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appResourcesService"></param>
        public static void UseAppResourcesFlash(this IApplicationBuilder app, IAppResourcesService appResourcesService)
        {
            app.Map(
                AppEndpointsConst.AppResourcesFlashPath,
                (appBuilder) =>
                {
                    appBuilder.Run(async (context) =>
                    {
                        if (context.Request.Method == HttpMethods.Options)
                        {
                            context.Response.AddCorsToAllowAll();
                            return;
                        }
                        if (context.Request.Method != HttpMethods.Get)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            return;
                        }

                        context.Response.AddCorsToAllowAll();
                        context.Response.Headers.Add("Content-Type", "application/json");
                        try
                        {
                            var htmlParsed = bool.TryParse(context.Request.Query["html"], out var html);
                            if (IsResourceRequest(context.Request))
                            {
                                await context.Response.WriteAsync(await appResourcesService.GetAsync(
                                    context.Request.Query["cacheId"],
                                    WebUtility.UrlDecode(context.Request.Query["resourcePath"]),
                                    htmlParsed && html
                                ));

                            }

                        }
                        catch (Exception e)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            await context.Response.WriteAsync(
$@"{{
    errorCode: ""{AppErrorCodesConst.InternalServerErrorResourcesFlashRetrievingCache}"",
    message: ""{e.Message}""
}}");
                        }
                    });
                }
            );
        }

        #region Helpers

        private static void AddCorsToAllowAll(this HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET");
            response.Headers.Add("Access-Control-Max-Age", "3600");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");
        }


        private static bool IsResourceRequest(HttpRequest req)
        {
            // We only accept requests that have particular values as part of the request. Everything else is 
            // not a proper FlashResource request.
            return (((req.Query.Count == 3)
                     && (req.Query.ContainsKey("resourcePath")
                         && req.Query.ContainsKey("cacheId")
                         && req.Query.ContainsKey("html"))
                     || ((req.Query.Count == 2)
                         && (req.Query.ContainsKey("resourcePath")
                             && req.Query.ContainsKey("cacheId")
                         ))));

        }

        #endregion Helpers
    }
}