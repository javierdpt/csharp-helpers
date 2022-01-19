using System;

namespace Api.Infrastructure.ExtensionMethods
{
    /// <summary>
    /// Extension methods for DateTime objects
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns a DateTime object with the time set to midnight
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToMidnightTime(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }
    }
}
