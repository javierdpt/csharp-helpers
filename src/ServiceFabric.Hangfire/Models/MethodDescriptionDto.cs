using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HangfireService.Infrastructure.Extensions;

namespace HangfireService.Models
{
    public class MethodDescriptionDto
    {
        public MethodDescriptionDto()
        { }

        public MethodDescriptionDto(MethodInfo methodInfo)
        {
            MethodName = methodInfo.Name;
            MethodReturnType = methodInfo.ReturnType.GetGenericType();
            MethodParameters = methodInfo.GetParameters().Select(p =>
                new ParameterDescriptionDto(p));
        }

        public string MethodReturnType { get; set; }
        public IEnumerable<ParameterDescriptionDto> MethodParameters { get; set; }
        public string MethodName { get; set; }
    }
}