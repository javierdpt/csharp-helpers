using Assurant.DynamicFulfillment.Authorization.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Diagnostics.CodeAnalysis;

namespace Assurant.DynamicFulfillment.Authorization.WebApi.Infrastructure.Features
{
    [ExcludeFromCodeCoverage]
    public class GenericConfigurationControllerModelConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (!controller.ControllerType.IsGenericType || controller.ControllerType.GetGenericTypeDefinition() != typeof(ConfigController<,>))
            {
                return;
            }

            var entityType = controller.ControllerType.GenericTypeArguments[0];
            var controllerName = $"{FormatControllerName(entityType.Name)}";
            if (!controllerName.EndsWith("s"))
            {
                controllerName = $"{controllerName}s";
            }
            controller.ControllerName = controllerName;
            controller.RouteValues["Controller"] = controllerName;
        }

        private static string FormatControllerName(string entityName)
        {
            const string suffix = "Config";
            return entityName.EndsWith(suffix)
                ? entityName.Substring(0, entityName.Length - suffix.Length)
                : entityName;
        }
    }
}