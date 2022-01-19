using .Lease.Model;
using .Lease.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageType = .Lease.Model.ImageType;
using PhoneType = .Lease.Model.PhoneType;
using Product = .Lease.Model.Product;
using Program = .Lease.Model.Program;

namespace .Lease.Data.Infrastructure.SampleData
{
    public partial class LeaseDataInitializer
    {
        public const int MaxInternalSysmtems = 5;
        public const int MaxSoftwareVendors = 4;
        public const int MaxPrograms = 10;
        public const int MaxProducts = 10;
        public const int MaxCoverageProviders = 10;
        public const int MaxCoverageProviderPrograms = 10;
        public const int MaxImageTypes = 4;
        public const int MaxCommunities = 5;
        public const int MaxCommunitieUnitRoesterVendorMappings = 200;
        public const int MaxCommunitySystemCredentials = 200;
        public const int MaxCommunityUnitRosterUnitsPerAddress = 15;
        public const int MaxCommunityUnitRosterAddressesPerCommunity = 10;
        public const int MaxPhoneTypes = 6;
        public const int MaxResidents = 200;
        public const int MaxLessee = 100;
        public const int MaxProductItemsPerLease = 6;
        public const int MaxLinkPerCoverageRequirement = 4;
    }

    public partial class LeaseDataInitializer
    {
        public async Task InitializeDatabaseAsync(IServiceProvider serviceProvider, string environment)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<LeaseDbContext>();
                var dbEnsureCreated = await dbContext.Database.EnsureCreatedAsync();

                //if (dbEnsureCreated)
                //{
                //    await InsertSampleData(dbContext, environment);
                //}
                //else
                //{
                //    var records = await dbContext.Residents.CountAsync() + await dbContext.Leases.CountAsync();
                //    if (records == 0)
                //    {
                //        await InsertSampleData(dbContext, environment);
                //    }
                //}
                //await AddImagesToOutsideInsurance(dbContext, new Random());
            }
        }

        private static async Task InsertSampleData(LeaseDbContext dbContext, string environment)
        {
            var rand = new Random();
            Community devCommunity = null;
            if (environment == "Local")
            {
                // Carrington Park - RAIT Residential
                // MFH-Dev 15953
                //    -Mod 13035
                const int testCommunityId = 13035;
                devCommunity = await dbContext.Communities
                    .Include(item => item.CommunityImages).ThenInclude(i => i.ImageType)
                    .Include(item => item.Addresses)
                    .ThenInclude(item => item.Units)
                    .FirstOrDefaultAsync(item => item.Id == testCommunityId);

                // Add image to the community
                await AddImagesToCommunity(dbContext, devCommunity);
            }

            #region InternalSystems

            List<Model.InternalSystem> internalSystems;
            if (await dbContext.InternalSystems.CountAsync() == 0)
            {
                internalSystems = Enumerable.Range(1, rand.Next(2, MaxInternalSysmtems)).Select(i =>
                    SampleDataUtils.GetRandom_InternalSystem(rand, 0, i)).ToList();

                AddItems(dbContext.InternalSystems, internalSystems);
                await dbContext.SaveChangesAsync();
            }
            else
                internalSystems = await dbContext.InternalSystems.ToListAsync();

            #endregion InternalSystems

            #region SoftwareVendors

            List<SoftwareVendor> vendors;
            if (await dbContext.SoftwareVendors.CountAsync() == 0)
            {
                vendors = Enumerable.Range(1, rand.Next(2, MaxSoftwareVendors)).Select(i =>
                    SampleDataUtils.GetRandom_SoftwareVendor(rand, 0, i)).ToList();
                AddItems(dbContext.SoftwareVendors, vendors);
                await dbContext.SaveChangesAsync();
            }
            else
                vendors = await dbContext.SoftwareVendors.ToListAsync();

            #endregion SoftwareVendors

            #region Programs

            List<Program> programs;
            if (await dbContext.Programs.CountAsync() == 0)
            {
                programs = Enumerable.Range(4, rand.Next(5, MaxPrograms)).Select(i =>
                    SampleDataUtils.GetRandom_Program(rand, 0, i)).ToList();

                AddItems(dbContext.Programs, programs);
                await dbContext.SaveChangesAsync();
            }
            else
                programs = await dbContext.Programs.ToListAsync();

            #endregion Programs

            #region Products

            List<Product> products;
            if (await dbContext.Products.CountAsync() == 0)
            {
                products = Enumerable.Range(4, rand.Next(5, MaxProducts)).Select(i =>
                    SampleDataUtils.GetRandom_Product(rand, 0, i, programs)).ToList();

                AddItems(dbContext.Products, products);
                await dbContext.SaveChangesAsync();
            }
            else
                products = await dbContext.Products.ToListAsync();

            #endregion Products

            #region PhoneTypes

            List<PhoneType> phoneTypes;
            if (await dbContext.PhoneTypes.CountAsync() == 0)
            {
                phoneTypes = new List<PhoneType>
                {
                    new PhoneType {Name = "Home"},
                    new PhoneType {Name = "Cell"},
                    new PhoneType {Name = "Office"},
                    new PhoneType {Name = "Other"}
                };
                AddItems(dbContext.PhoneTypes, phoneTypes);
                await dbContext.SaveChangesAsync();
            }
            else
                phoneTypes = await dbContext.PhoneTypes.ToListAsync();

            #endregion PhoneTypes

            #region ImageTypes

            List<ImageType> imageTypes;
            if (await dbContext.ImageTypes.CountAsync() == 0)
            {
                imageTypes = Enumerable.Range(1, rand.Next(2, MaxImageTypes)).Select(i =>
                    SampleDataUtils.GetRandom_ImageType(rand, 0, i)).ToList();
                AddItems(dbContext.ImageTypes, imageTypes);
                await dbContext.SaveChangesAsync();
            }
            else
                imageTypes = await dbContext.ImageTypes.ToListAsync();

            #endregion ImageTypes

            #region Communities & CommunityImages

            List<Community> communities;
            if (await dbContext.Communities.CountAsync() == 0)
            {
                communities = Enumerable.Range(1, rand.Next(5, MaxCommunities)).ToList().Select(i =>
                    SampleDataUtils.GetRandom_Community(rand, 0, i, vendors, imageTypes)
                ).ToList();
                AddItems(dbContext.Communities, communities);
                await dbContext.SaveChangesAsync();
            }
            else
                communities = await dbContext.Communities.ToListAsync();

            #endregion Communities & CommunityImages

            #region CommunityUnitRosterUnitVendorMapping

            if (await dbContext.CommunityUnitRosterUnitVendorMappings.CountAsync() == 0)
            {
                var index = 1;
                var communityUnitRosterUnitVendorMappings = Enumerable.Range(5, rand.Next(100, MaxCommunitieUnitRoesterVendorMappings)).Select(i =>
                    SampleDataUtils.GetRandom_CommunityUnitRosterUnitVendorMapping(rand, index++, i)).ToList();
                AddItems(dbContext.CommunityUnitRosterUnitVendorMappings, communityUnitRosterUnitVendorMappings);
                await dbContext.SaveChangesAsync();
            }

            #endregion CommunityUnitRosterUnitVendorMapping

            #region CommunitySystemCredential

            if (await dbContext.CommunitySystemCredentials.CountAsync() == 0)
            {
                var communitySystemCredentials = Enumerable.Range(1, rand.Next(100, MaxCommunitySystemCredentials)).Select(i =>
                    SampleDataUtils.GetRandom_CommunitySystemCredential(rand, 0, i, communities, internalSystems)).ToList();
                AddItems(dbContext.CommunitySystemCredentials, communitySystemCredentials);
                await dbContext.SaveChangesAsync();
            }

            #endregion CommunitySystemCredential

            #region CommunityUnitRosterAddress

            List<CommunityUnitRosterAddress> addresses;
            if (devCommunity != null)
            {
                addresses = new List<CommunityUnitRosterAddress>();
                if (devCommunity.Addresses.Count == 0)
                {
                    var index = 1;
                    Enumerable.Range(1, rand.Next(2, MaxCommunityUnitRosterAddressesPerCommunity)).Select(i =>
                        SampleDataUtils.GetRandom_CommunityUnitRosterAddress(rand, index++, i, devCommunity)).ToList().ForEach(addresses.Add);

                    AddItems(dbContext.CommunityUnitRosterAddresses, addresses);
                    await dbContext.SaveChangesAsync();
                }
                else
                    addresses = devCommunity.Addresses.ToList();
            }
            else if (await dbContext.CommunityUnitRosterAddresses.CountAsync() == 0)
            {
                addresses = new List<CommunityUnitRosterAddress>();
                communities.ForEach(community =>
                {
                    var index = 1;
                    Enumerable.Range(1, rand.Next(2, MaxCommunityUnitRosterAddressesPerCommunity)).Select(i =>
                        SampleDataUtils.GetRandom_CommunityUnitRosterAddress(rand, index++, i, community)).ToList().ForEach(addresses.Add);
                });
                AddItems(dbContext.CommunityUnitRosterAddresses, addresses);
                await dbContext.SaveChangesAsync();
            }
            else
                addresses = dbContext.CommunityUnitRosterAddresses.ToList();

            #endregion CommunityUnitRosterAddress

            #region CommunityUnitRosterUnit

            List<CommunityUnitRosterUnit> units;
            if (devCommunity != null)
            {
                units = new List<CommunityUnitRosterUnit>();
                if (!devCommunity.Addresses.SelectMany(item => item.Units).Any())
                {
                    var index = 1;
                    addresses.ForEach(address =>
                    {
                        Enumerable.Range(1, rand.Next(5, MaxCommunityUnitRosterUnitsPerAddress)).Select(i =>
                            SampleDataUtils.GetRandom_CommunityUnitRosterUnit(rand, index++, i, address)).ToList().ForEach(units.Add);
                    });

                    AddItems(dbContext.CommunityUnitRosterUnits, units);
                    await dbContext.SaveChangesAsync();
                }
                else
                    units = devCommunity.Addresses.SelectMany(item => item.Units).ToList();
            }
            else if (await dbContext.CommunityUnitRosterUnits.CountAsync() == 0)
            {
                units = new List<CommunityUnitRosterUnit>();
                addresses.ForEach(address =>
                {
                    var index = 1;
                    Enumerable.Range(1, rand.Next(5, MaxCommunityUnitRosterUnitsPerAddress)).Select(i =>
                        SampleDataUtils.GetRandom_CommunityUnitRosterUnit(rand, index++, i, address)).ToList().ForEach(units.Add);
                });
                AddItems(dbContext.CommunityUnitRosterUnits, units);
                await dbContext.SaveChangesAsync();
            }
            else
                units = await dbContext.CommunityUnitRosterUnits.ToListAsync();

            #endregion CommunityUnitRosterUnit

            #region Leases

            List<Model.Lease> leases;
            if (await dbContext.Leases.CountAsync() == 0)
            {
                leases = new List<Model.Lease>();
                if (devCommunity != null)
                {
                    devCommunity.Addresses.SelectMany(a => a.Units).ToList().ForEach(unit =>
                    {
                        Enumerable.Range(1, rand.Next(1, 30)).ToList().Select(i =>
                            SampleDataUtils.GetRandom_Lease(rand, 0, i, unit)).ToList().ForEach(leases.Add);
                    });
                }
                else
                {
                    units.ForEach(unit =>
                    {
                        Enumerable.Range(1, rand.Next(1, 10)).ToList().Select(i =>
                            SampleDataUtils.GetRandom_Lease(rand, 0, i, unit)).ToList().ForEach(leases.Add);
                    });
                }

                AddItems(dbContext.Leases, leases);
                await dbContext.SaveChangesAsync();
            }
            else
                leases = await dbContext.Leases.ToListAsync();

            #endregion Leases

            #region LeaseResidents & ProductItems (Bond, InsurancePolicy) & OutsideInsurance & Residents & ResidentPhone

            List<LeaseResident> leaseResidents;
            var productItems = new List<ProductItem>();

            if (await dbContext.LeaseResidents.CountAsync() == 0)
            {
                leaseResidents = new List<LeaseResident>();

                foreach (var lease in leases)
                {
                    foreach (var i in Enumerable.Range(1, rand.Next(1, MaxProductItemsPerLease)))
                    {
                        var resident = SampleDataUtils.GetRandom_Resident(rand, 0, i, phoneTypes);

                        leaseResidents.Add(
                            SampleDataUtils.GetRandom_LeaseResident(rand, 0, resident, lease, productItems.LastOrDefault()));
                    }
                }

                AddItems(dbContext.ProductItems, productItems);

                leaseResidents = leaseResidents.GroupBy(x => new { x.Lease, x.Resident })
                    .Select(item =>
                    {
                        var tmp = item.First();
                        return new LeaseResident
                        {
                            Lease = tmp.Lease,
                            Resident = tmp.Resident,
                            ProductItem = tmp.ProductItem
                        };
                    }).ToList();

                leaseResidents.GroupBy(item => item.Lease).ToList().ForEach(gr =>
                {
                    var lst = gr.ToList();
                    var mainResident = lst[rand.Next(0, gr.Count())];
                    var childs = lst.Where(item => item.Resident != mainResident.Resident);
                    childs.ToList().ForEach(item =>
                    {
                        item.Parent = mainResident;
                    });
                });

                AddItems(dbContext.LeaseResidents, leaseResidents);
                await dbContext.SaveChangesAsync();
            }
            else
                leaseResidents = await dbContext.LeaseResidents.ToListAsync();

            #endregion LeaseResidents & ProductItems (Bond, InsurancePolicy) & OutsideInsurance & Residents & ResidentPhone

            #region CoverageRequirement

            List<CoverageRequirement> coverageRequirements;
            if (await dbContext.CoverageRequirements.CountAsync() == 0)
            {
                coverageRequirements = new List<CoverageRequirement>();
                foreach (var leaseResident in leaseResidents)
                {
                    coverageRequirements.Add(SampleDataUtils.GetRandom_CoverageRequirement(
                        rand, 0, leaseResident));
                }
                AddItems(dbContext.CoverageRequirements, coverageRequirements);
                await dbContext.SaveChangesAsync();

                // SettingUp Lease status
                var completedLeases = await dbContext.CoverageRequirements
                    .Include(item => item.LeaseResident)
                    .ThenInclude(item => item.Lease)
                    .Select(item => item.LeaseResident.Lease)
                    .ToListAsync();

                completedLeases.ForEach(item =>
                {
                    var isIncompleted = rand.Next() % 5 == 0;
                    if (!isIncompleted)
                    {
                        item.LeaseStatus = LeaseStatus.Completed;

                        var i = 0;
                        item.LeaseResidents.ToList().ForEach(lr =>
                        {
                            var isBond = rand.Next() % 7 == 0;
                            if (!isBond)
                                productItems.Add(
                                    SampleDataUtils.GetRandom_InsurancePolicy(rand, 0, i, products));
                            else
                                productItems.Add(
                                    SampleDataUtils.GetRandom_Bond(rand, 0, i, products));

                            lr.ProductItem = productItems.Last();

                            lr.CoverageRequirements.ToList().ForEach(cr =>
                            {
                                cr.CompletionMethod =
                                (isBond || productItems.Last() is InsurancePolicy &&
                                    ((InsurancePolicy)productItems.Last()).OutsideInsurances.Count == 0)
                                    ? CoverageRequirementCompletionMethod.PurchasedInternalProduct
                                    : CoverageRequirementCompletionMethod.PurchasedExternalProduct;
                                cr.CompletedDateOnUtc = DateTime.UtcNow;
                            });

                            i++;
                        });
                    }
                });
                await dbContext.SaveChangesAsync();
            }
            else
                coverageRequirements = await dbContext.CoverageRequirements.ToListAsync();

            #endregion CoverageRequirement

            #region Links

            if (await dbContext.Links.CountAsync() == 0)
            {
                var links = new List<Link>();
                coverageRequirements.ForEach(coverageRequirement =>
                {
                    Enumerable.Range(1, rand.Next(1, 3)).Select(i =>
                        SampleDataUtils.GetRandom_Link(rand, 0, coverageRequirement)).ToList().ForEach(links.Add);
                });
                AddItems(dbContext.Links, links);
                await dbContext.SaveChangesAsync();
            }

            #endregion Links
        }

        private static void AddItems<T>(DbSet<T> dbSet, List<T> itemsToAdd) where T : class
        {
            dbSet.AddRange(itemsToAdd);
        }

        private static async Task AddImagesToCommunity(LeaseDbContext dbContext, Community devCommunity)
        {
            // path to local images
            var logoPath = "c:\\6.jpg";
            var bannPath = "c:\\5.jpg";

            using (var output = new FileStream(logoPath, FileMode.Open, FileAccess.Read))
            {
                var imageLogo = new byte[output.Length];
                await output.ReadAsync(imageLogo, 0, imageLogo.Length);

                var logo = devCommunity.CommunityImages.FirstOrDefault(item => item.ImageType.Id == (int)Model.Enums.ImageType.Logo);
                if (logo == null)
                {
                    logo = new CommunityImage
                    {
                        CommunityId = devCommunity.Id,
                        Content = imageLogo,
                        ImageTypeId = (int)Model.Enums.ImageType.Logo,
                        IsVerified = true,
                        VerifiedBy = "Javier D Perez",
                        VerifiedOn = DateTime.Now
                    };
                    dbContext.CommunityImages.Add(logo);
                }
                else
                    logo.Content = imageLogo;
            }

            using (var output = new FileStream(bannPath, FileMode.Open, FileAccess.Read))
            {
                var imageBanner = new byte[output.Length];
                await output.ReadAsync(imageBanner, 0, imageBanner.Length);

                var bann = devCommunity.CommunityImages.FirstOrDefault(item => item.ImageType.Id == (int)Model.Enums.ImageType.Banner);
                if (bann == null)
                {
                    bann = new CommunityImage
                    {
                        CommunityId = devCommunity.Id,
                        Content = imageBanner,
                        ImageTypeId = (int)Model.Enums.ImageType.Banner,
                        IsVerified = true,
                        VerifiedBy = "Javier D Perez",
                        VerifiedOn = DateTime.Now
                    };
                    dbContext.CommunityImages.Add(bann);
                }
                else
                    bann.Content = imageBanner;
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task AddImagesToOutsideInsurance(LeaseDbContext dbContext, Random rand)
        {
            // path to local images
            var pdf = "c:\\tmp\\oisample.pdf";
            var image = "c:\\tmp\\insurancedec.jpg";

            using (var pdfOutput = new FileStream(pdf, FileMode.Open, FileAccess.Read))
            {
                using (var imageOutput = new FileStream(image, FileMode.Open, FileAccess.Read))
                {
                    var pdfBytes = new byte[pdfOutput.Length];
                    var imageBytes = new byte[imageOutput.Length];

                    var task1 = pdfOutput.ReadAsync(pdfBytes, 0, pdf.Length);
                    var task2 = imageOutput.ReadAsync(imageBytes, 0, pdf.Length);

                    await Task.WhenAll(task1, task2);

                    var outsideInsurances = await dbContext.OutsideInsurances.Where(oi => string.IsNullOrEmpty(oi.ContentType)).ToListAsync();

                    outsideInsurances.ForEach(item =>
                    {
                        if (rand.Next() % 2 == 0)
                        {
                            item.ContentType = "application/pdf";
                            item.Content = pdfBytes;
                        }
                        else
                        {
                            item.ContentType = "image/jpg";
                            item.Content = imageBytes;
                        }
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}

/*
    delete ResidentPhones
    delete OutsideInsurances
    delete CoverageRequirements
    delete ProductItems
    delete Links
    delete LeaseResidents
    delete Leases

    select count(*) from ResidentPhones
    select count(*) from OutsideInsurances
    select count(*) from CoverageRequirements
    select count(*) from ProductItems
    select count(*) from Links
    select count(*) from LeaseResidents
    select count(*) from Leases
 */