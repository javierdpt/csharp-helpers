using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GS.MFH.Common.Api;
using GS.MFH.CommunityData.Interfaces;
using GS.MFH.CommunityData.Model;
using GS.MFH.CommunityData.Model.Enums;
using GS.MFH.VendorIntegrationService.Infrastructure.Options;
using GS.MFH.VendorIntegrationService.Infrastructure.VendorImplementation;
using GS.MFH.VendorIntegrationService.Services.ProxyResolver;
using Microsoft.Extensions.Options;

namespace GS.MFH.VendorIntegrationService.Services.SoftwareVendorIntegration
{
    public class SoftwareVendorIntegrationService : ISoftwareVendorIntegrationService
    {
        private readonly IProxyResolverService _proxyResolver;
        private readonly IApiFactory _apiFactory;
        private readonly IOptions<ApiConfig> _apiConfig;
        private readonly ICommunityService _communityService;

        public SoftwareVendorIntegrationService(IProxyResolverService proxyResolver,
            IApiFactory apiFactory, IOptions<ApiConfig> apiConfig)
        {
            _proxyResolver = proxyResolver;
            _apiFactory = apiFactory;
            _apiConfig = apiConfig;
            _communityService = proxyResolver.CommunityService();
        }

        public async Task<TVendorImplementation> GetVendorIntegrationImplementation<TVendorImplementation>(
            int communityId,
            ProgramsEnum program
        ) where TVendorImplementation : class, IBaseIntegration
        {
            var community = await GetCommunityInfo(communityId);
            var credentials = GetCommunityCredentials(community);

            if (credentials == null || !credentials.Any())
            {
                throw new TypeLoadException($"Could not find an SoftwareVendor in {nameof(InitSoftwareVendorIntegration)} for CommunityId: {community.Id} for Renters Program.");
            }

            return InitSoftwareVendorIntegration<TVendorImplementation>(community, credentials, program);
        }

        #region Helpers

        private TVendorImplementation InitSoftwareVendorIntegration<TVendorImplementation>(
            Community community,
            List<ExternalSystemCredential> credentials,
            ProgramsEnum program
        ) where TVendorImplementation : IBaseIntegration
        {
            var vendorImplementationBaseType = typeof(TVendorImplementation);

            var types = (
                from type in Assembly.GetAssembly(vendorImplementationBaseType).GetTypes()
                where type.IsClass && !type.IsAbstract && vendorImplementationBaseType.IsAssignableFrom(type)
                select type
            ).ToArray();

            var credTypeName =
                community.CommunityPrograms
                    .FirstOrDefault(cp => cp.ProgramId == (int)program)
                    ?.SoftwareVendor
                    ?.Description
                ??
                community.Company.CompanyPrograms
                    .FirstOrDefault(cp => cp.ProgramId == (int)program)
                    ?.SoftwareVendor
                    ?.Description;

            if (string.IsNullOrEmpty(credTypeName))
            {
                throw new TypeLoadException($"Could not find an SoftwareVendor in {nameof(InitSoftwareVendorIntegration)} for CommunityId: {community.Id} for Renters Program.");
            }

            var typeToCreate = types.FirstOrDefault(t =>
                t.Name.IndexOf(credTypeName, StringComparison.InvariantCultureIgnoreCase) != -1
            );

            return typeToCreate != null
                ? (TVendorImplementation)Activator.CreateInstance(
                    typeToCreate,
                    community,
                    credentials
                        .Where(c => c.ExternalSystemInterface.SoftwareVendor.Description == credTypeName)
                        .ToList(),
                    _apiFactory,
                    _proxyResolver,
                    _apiConfig
                )
                : throw new TypeLoadException($"Could not find an implementation for the credential type: {credTypeName}");
        }

        private Task<Community> GetCommunityInfo(int communityId)
        {
            return _communityService.Find(
                    c => c.Id == communityId,
                    new[]
                    {
                        "CommunityProducts.Product",
                        "CommunityPrograms.SoftwareVendor",
                        "Company.CompanyProducts",
                        "Company.CompanyPrograms.SoftwareVendor",
                        "Company.CompanyExternalSystemCredentials.ExternalSystemCredential.ExternalSystemInterface.SoftwareVendor",
                        "CommunityExternalSystemCredentials.ExternalSystemCredential.ExternalSystemInterface.SoftwareVendor",
                        "DwellingType"
                    }
                );
        }

        private List<ExternalSystemCredential> GetCommunityCredentials(Community community)
        {
            var commCred = community
                .CommunityExternalSystemCredentials
                .Select(communityExtSysCred => communityExtSysCred.ExternalSystemCredential)
                .ToList();
            var compCred = community
                .Company
                .CompanyExternalSystemCredentials
                .Select(compExtSysCred => compExtSysCred.ExternalSystemCredential)
                .ToList();

            var intTypes = commCred
                .Select(cc => cc.ExternalSystemInterfaceId)
                .Union(compCred.Select(cpc => cpc.ExternalSystemInterfaceId));

            return intTypes
                .Select(t =>
                    commCred.FirstOrDefault(c => c.ExternalSystemInterfaceId == t) ??
                    compCred.FirstOrDefault(cp => cp.ExternalSystemInterfaceId == t)
                )
                .ToList();
        }

        #endregion Helpers
    }
}