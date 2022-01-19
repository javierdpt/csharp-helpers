using System;

namespace .RentersInsurance.Api.Infrastructure.Extensions
{
    /// <summary>
    /// DateTime extensions methods
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        ///  DateTime Epoch date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static double GetUnixEpoch(this DateTime dateTime)
        {
            var unixTime = dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixTime.TotalSeconds;
        }
    }
}