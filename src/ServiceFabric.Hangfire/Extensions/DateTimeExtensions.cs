using System;

namespace HangfireService.Infrastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset GetDateTimeOffsetForTimeZone(
            this DateTime dateTime,
            TimeZoneInfo timeZone)
        {
            var dateTimeUnspecified = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTimeUnspecified, timeZone);

            return new DateTimeOffset(
                dateTimeUnspecified,
                dateTime.Subtract(utcDateTime)
            );
        }
    }
}