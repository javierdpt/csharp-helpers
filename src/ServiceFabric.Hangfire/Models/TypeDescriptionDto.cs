using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HangfireService.Models
{
    public class TypeDescriptionDto
    {
        public TypeDescriptionDto()
        { }

        public TypeDescriptionDto(TypeInfo typeInfo)
        {
            InterfaceName = typeInfo.FullName;
            InterfaceMethods = typeInfo.DeclaredMethods
                .Select(m => new MethodDescriptionDto(m));
        }

        public string InterfaceName { get; set; }
        public IEnumerable<MethodDescriptionDto> InterfaceMethods { get; set; }
    }
}