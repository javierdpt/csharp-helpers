using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GS.MFH.Lease.Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// Model State Extensions methods
    /// </summary>
    public static class ModelStateExtensions
    {
        /// <summary>
        /// Return the errors messages concatenated
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static string GetErrorMessagesString(this ModelStateDictionary modelState)
        {
            var resp = new Dictionary<string, string>();
            modelState.Keys.ToList().ForEach(key =>
            {
                if (modelState[key].Errors.Any())
                {
                    resp[key] = string.Join(" ", modelState[key].Errors.Select(e => e.ErrorMessage));
                }
            });
            return JsonConvert.SerializeObject(new { ValidationFieldsErrors = resp });
        }
    }
}