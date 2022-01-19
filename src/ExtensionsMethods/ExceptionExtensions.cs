using System;

namespace Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception ToFriendlySerializerException(this Exception e)
        {
            return new Exception(e.Message,
                new Exception(e.StackTrace,
                    new Exception($"Message: {e.InnerException?.Message} \n\nStack Trace:{e.InnerException?.StackTrace}")));
        }
    }
}