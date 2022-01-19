using System.Collections.Generic;
using Newtonsoft.Json;

namespace .PMC.Portal.Sitefinity.Infrastructure.GoogleRecaptcha
{
    internal class CaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}