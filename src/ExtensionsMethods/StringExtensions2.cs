using System;

namespace GS.MFH.RentersInsurance.Api.Infrastructure.Extensions
{
    /// <summary>
    /// String extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Parse a string to boolean
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool ParseBoolean(this string input)
        {
            return input != null && (
                       input.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
                       input.Equals("y", StringComparison.CurrentCultureIgnoreCase) ||
                       input.Equals("true", StringComparison.CurrentCultureIgnoreCase) ||
                       input.Equals("1", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}