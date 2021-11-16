using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using System;
using System.Linq;

namespace HangfireService.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomDisplayJobNameAttribute : JobDisplayNameAttribute
    {
        private readonly string[] _replace;
        private readonly string[] _replaceWith;

        public CustomDisplayJobNameAttribute(string displayName, string[] replace = null, string[] replaceWith = null) : base(displayName)
        {
            _replace = replace;
            _replaceWith = replaceWith;
        }

        public override string Format(DashboardContext context, Job job)
        {
            var args = job.Args
                .ToArray()
                .Select(a =>
                    a == null
                        ? null
                        : a is Type type ? type.Name : a.ToString() as object
                )
                .ToArray();

            var resp = string.Format(
                DisplayName,
                args
            );

            try
            {
                for (var i = 0; i < _replace.Length; i++)
                    resp = resp.Replace(_replace[i], _replaceWith[i]);
                return resp;
            }
            catch (Exception)
            {
                return resp;
            }
        }
    }
}