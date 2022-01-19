using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Jobs.HangfireService.Infrastructure.Extensions
{
    public static class HttpContextExtension
    {
        public static async Task GetHandleUnauthorizedResponse(this HttpContext httpContext, Exception e)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            if (httpContext.Request.IsAjaxRequest())
            {
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    ErrorMessage = e.Message,
                    e.StackTrace
                }));
                return;
            }
            httpContext.Response.ContentType = "text/html";
            await httpContext.Response.WriteAsync(GetHtmlPage(e.Message, "#ef5a00"));
        }

        #region Helpers

        private static string GetHtmlPage(string message, string fontColor)
        {
            return $@"
                    <html>
                        <head>
                        </head>
                        <body style=""font-family: -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif,'Apple Color Emoji','Segoe UI Emoji','Segoe UI Symbol';"">
                            <div style=""position: fixed; width: 600px; text-align: center; top: 50%; left: 50%; margin-top: -135px; margin-left: -300px;"">
                                <h1 style=""color: {fontColor}; font-weight: 100;"">An error occurred processing your authentication</h1>
                                {message}
                            </div>
                        </body>
                    </html >";
        }

        #endregion Helpers
    }
}