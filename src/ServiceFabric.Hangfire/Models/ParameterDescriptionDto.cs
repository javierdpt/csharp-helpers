using System;
using System.Linq;
using System.Reflection;
using HangfireService.Infrastructure.Extensions;

namespace HangfireService.Models
{
    public class ParameterDescriptionDto
    {
        public ParameterDescriptionDto()
        { }

        public ParameterDescriptionDto(ParameterInfo parameterInfo)
        {
            ParameterPosition = parameterInfo.Position;
            ParameterName = parameterInfo.Name;
            ParameterType = parameterInfo.ParameterType.GetGenericType();
        }

        public int ParameterPosition { get; set; }
        public string ParameterType { get; set; }
        public string ParameterName { get; set; }

        
    }
}