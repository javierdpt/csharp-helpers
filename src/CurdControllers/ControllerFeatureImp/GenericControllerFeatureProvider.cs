using Assurant.DynamicFulfillment.Authorization.Core.Config.Contracts;
using Assurant.DynamicFulfillment.Authorization.WebApi.Controllers;
using Assurant.DynamicFulfillment.Common.Configuration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Assurant.DynamicFulfillment.Authorization.WebApi.Infrastructure.Features
{
    [ExcludeFromCodeCoverage]
    public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            feature.Controllers.Add(GetConfigControllerTypeInfo<SourceSystemConfig, SourceSystemConfigDto>());
            feature.Controllers.Add(GetConfigControllerTypeInfo<FulfillmentOptionConfig, FulfillmentOptionConfigDto>());
            feature.Controllers.Add(GetConfigControllerTypeInfo<PriceListConfig, PriceListConfigDto>());
            feature.Controllers.Add(GetConfigControllerTypeInfo<VendorTicketStatusConfig, VendorTicketStatusMappingDto>());
            feature.Controllers.Add(GetConfigControllerTypeInfo<VendorConfig, VendorConfigDto>());
        }

        private static TypeInfo GetConfigControllerTypeInfo<TEntity, TDto>() =>
            typeof(ConfigController<,>).MakeGenericType(typeof(TEntity), typeof(TDto)).GetTypeInfo();
    }
}