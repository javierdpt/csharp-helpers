using Lease.Model;
using Lease.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using ImageType = .Lease.Model.ImageType;
using PhoneType = .Lease.Model.PhoneType;
using Product = .Lease.Model.Product;
using Program = .Lease.Model.Program;

namespace .Lease.Data.Infrastructure.SampleData
{
    public class SampleDataUtils
    {
        #region InternalSystems

        public static Model.InternalSystem GetRandom_InternalSystem(
            Random rand,
            int? id,
            int i)
        {
            var res = new Model.InternalSystem
            {
                Name = $"InternalSystem{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion InternalSystems

        #region Bond

        public static Bond GetRandom_Bond(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<Product> products = null
            )
        {
            var res = new Bond
            {
                CreatedOnUtc = DateTime.UtcNow,
                Product = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    products,
                    () => GetRandom_Product(rand, id, rand.Next(1, 100))),
                EffectiveDateOnUtc = DateTime.Now.AddDays(rand.Next(1, 100)),
                ExpirationDateOnUtc = DateTime.Now.AddDays(rand.Next(100, 360)),
                Number = $"123456{i}",
                Status = rand.Next() % 2 == 0 ? ProductItemStatus.Active : ProductItemStatus.Cancelled,
                CoverageProvider = "Assurant"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion Bond

        #region InsurancePolicy

        public static InsurancePolicy GetRandom_InsurancePolicy(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<Product> products = null
           )
        {
            var effectiveDate = GetRandomDatePass(rand, 30);
            var ipValidated = rand.Next() % 6 != 0;
            var hasOutsideInsurance = rand.Next() % 2 == 0;
            var res = new InsurancePolicy
            {
                CreatedOnUtc = DateTime.UtcNow,
                Product = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    products,
                    () => GetRandom_Product(rand, id, rand.Next(1, 1000))),
                EffectiveDateOnUtc = effectiveDate,
                ExpirationDateOnUtc = effectiveDate.AddYears(1),
                Number = $"RN123456{i}",
                Status = rand.Next() % 2 == 0 ? ProductItemStatus.Active : ProductItemStatus.Cancelled,
                CoverageProvider = "Assurant",
                LiabilityCoverageAmount = 100000,
                PersonalPropertyCoverageAmount = 10000
            };

            AddId(rand, id, ref res);

            if (hasOutsideInsurance)
            {
                res.OutsideInsurances = Enumerable.Range(1, rand.Next(2, 4)).Select(j =>
                {
                    var oi = new OutsideInsurance
                    {
                        InsurancePolicy = res,
                        InsurancePolicyId = res.Id,
                        Content = new byte[] { 0 },
                        UploadedDateOnUtc = GetRandomDate(rand),
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    if (!ipValidated)
                        return oi;

                    oi.Validated = true;
                    oi.ValidatedDateOnUtc = GetRandomDate(rand);

                    return oi;
                }).ToList();
            }

            return res;
        }

        #endregion InsurancePolicy

        #region Product

        public static Product GetRandom_Product(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<Program> programs = null)
        {
            var res = new Product
            {
                Program = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    programs,
                    () => GetRandom_Program(rand, id, rand.Next(1, 100))),
                Description = $"ProductName{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion Product

        #region Program

        public static Program GetRandom_Program(Random rand, int? id, int i)
        {
            var res = new Program
            {
                Description = $"ProgramName{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion Program

        #region Resident

        public static Resident GetRandom_Resident(
            Random rand,
            int? id,
            int? i,
            IReadOnlyList<PhoneType> phoneTypes = null)
        {
            var firstname = GetFirstname(rand);
            var middlename = GetFirstname(rand);
            var lastName = GetLastname(rand);
            var res = new Resident
            {
                CreatedOnUtc = DateTime.UtcNow,
                VendorId = $"VID{rand.Next(10000, 99999)}",
                Email = $"{firstname.ToLower()}.{lastName.ToLower()}@domain.com",
                FirstName = firstname,
                MiddleName = middlename,
                LastName = lastName,
                VendorStatus = $"Status{rand.Next(1, 5)}",
                ResidentPhones = Enumerable.Range(1, rand.Next(1, 4)).Select(j =>
                    GetRandom_ResidentPhone(rand, id, j, phoneTypes)).ToList()
            };

            AddId(rand, id, ref res);
            return res;
        }

        #endregion Resident

        #region ResidentPhone

        public static ResidentPhone GetRandom_ResidentPhone(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<PhoneType> phoneTypes = null)
        {
            var res = new ResidentPhone
            {
                PhoneType = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    phoneTypes,
                    () => GetRandom_PhoneType(rand, null)),
                Number = rand.Next(000000000, Int32.MaxValue).ToString()
            };

            AddId(rand, id, ref res);
            return res;
        }

        #endregion ResidentPhone

        #region LeaseResident

        public static LeaseResident GetRandom_LeaseResident(
            Random rand,
            int? id,
            Resident resident = null,
            Model.Lease lease = null,
            ProductItem productItem = null)
        {
            ProductItem randProductItem;
            if (rand.Next() % 7 == 0)
                randProductItem = GetRandom_Bond(rand, null, rand.Next(1, 1000));
            else
                randProductItem = GetRandom_InsurancePolicy(rand, null, rand.Next(1, 1000));

            var res = new LeaseResident
            {
                Id = rand.Next(1, 1000),
                Lease = GetItem_IfNull_GetRandomItem(
                    lease,
                    () => GetRandom_Lease(rand, null, rand.Next(1, 1000), addLeaseResidents: false)),
                Resident = GetItem_IfNull_GetRandomItem(
                    resident,
                    () => GetRandom_Resident(rand, null, rand.Next(1, 1000))),
                ProductItem = lease != null && lease.LeaseStatus == LeaseStatus.Completed ? GetItem_IfNull_GetRandomItem(
                    productItem,
                    () => randProductItem) : randProductItem,
                CommunicationMethod = CommunicationMethod.Email,
            };

            res.CoverageRequirements = Enumerable.Range(1, 1)
                .Select(r => GetRandom_CoverageRequirement(rand, null, res)).ToList();
            res.Lease.LeaseResidents.Add(res);

            return res;
        }

        #endregion LeaseResident

        #region Lease

        public static Model.Lease GetRandom_Lease(
            Random rand,
            int? id,
            int? i,
            CommunityUnitRosterUnit unit = null,
            DateTime? createdOnDate = null,
            bool addLeaseResidents = true)
        {
            var from = rand.Next(1, 60);

            var fromDate = DateTime.Now.AddDays(from);
            var singInDate = fromDate.AddDays(-1 * rand.Next(1, 30));

            var res = new Model.Lease
            {
                Unit = GetItem_IfNull_GetRandomItem(
                    unit,
                    () => GetRandom_CommunityUnitRosterUnit(rand, null, rand.Next(1, 1000))),
                FromDateUtc = fromDate,
                MoveInDateUtc = fromDate.AddDays(rand.Next(0, 15)),
                SignInDateUtc = singInDate,
                ToDateUtc = fromDate.AddYears(1),
                CreatedOnUtc = createdOnDate ?? GetRandomDatePass(rand, 180),
                LeaseStatus = LeaseStatus.Pending
            };

            if (addLeaseResidents)
            {
                res.LeaseResidents = Enumerable.Range(1, 4)
                    .Select(r => GetRandom_LeaseResident(rand, null, null, res)).ToList();
            }

            AddId(rand, id, ref res);
            return res;
        }

        #endregion Lease

        #region PhoneTypes

        public static PhoneType GetRandom_PhoneType(
            Random rand,
            int? id)
        {
            var phoneTypes = new List<PhoneType>
            {
                new PhoneType {Name = "Home"},
                new PhoneType {Name = "Cell"},
                new PhoneType {Name = "Office"},
                new PhoneType {Name = "Other"}
            };

            var res = phoneTypes[rand.Next(0, phoneTypes.Count)];
            AddId(rand, id, ref res);

            return res;
        }

        #endregion PhoneTypes

        #region CommunityUnitRosterUnit

        public static CommunityUnitRosterUnit GetRandom_CommunityUnitRosterUnit(
            Random rand,
            int? id,
            int i,
            CommunityUnitRosterAddress address = null)
        {
            var res = new CommunityUnitRosterUnit
            {
                Address = GetItem_IfNull_GetRandomItem(
                    address,
                    () => GetRandom_CommunityUnitRosterAddress(rand, null, rand.Next(1, 1000))),
                UnitNumber = $"{i}",
                IsActive = rand.Next() % 2 == 0
            };

            AddId(rand, id, ref res);
            return res;
        }

        #endregion CommunityUnitRosterUnit

        #region Community

        public static Community GetRandom_Community(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<SoftwareVendor> vendors = null,
            IReadOnlyList<ImageType> imageTypes = null)
        {
            var res = new Community
            {
                Name = $"Community {i}",
                Phone = rand.Next(100000000, 999999999).ToString(),
                Email = $"community{i}@domain.com",
                SoftwareVendor = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    vendors,
                    () => GetRandom_SoftwareVendor(rand, null, rand.Next(1, 1000))),
                CommunityImages = Enumerable.Range(1, rand.Next(1, 3)).Select(j =>
                    GetRandom_CommunityImage(rand, id, j, imageTypes)).ToList()
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion Community

        #region CommunityUnitRosterUnitVendorMapping

        public static CommunityUnitRosterUnitVendorMapping GetRandom_CommunityUnitRosterUnitVendorMapping(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<Program> programs = null)
        {
            var res = new CommunityUnitRosterUnitVendorMapping
            {
                CreatedOnUtc = GetRandomDate(rand),
                LastModifiedOnUtc = GetRandomDate(rand),
                VendorUnitId = rand.Next(10000, 100000).ToString(),
                VendorPropertyId = rand.Next(10000, 100000).ToString(),
                VendorAdditionalId = rand.Next(100, 1000).ToString()
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion CommunityUnitRosterUnitVendorMapping

        #region CommunitySystemCredential

        public static CommunitySystemCredential GetRandom_CommunitySystemCredential(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<Community> communities = null,
            IReadOnlyList<Model.InternalSystem> internalSystems = null)
        {
            var res = new CommunitySystemCredential
            {
                Community = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    communities,
                    () => GetRandom_Community(rand, null, rand.Next(1, 20))),
                InternalSystem = GetRandomItemArray(rand, internalSystems),
                UserId = $"UserId{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion CommunitySystemCredential

        #region CommunityUnitRosterAddress

        public static CommunityUnitRosterAddress GetRandom_CommunityUnitRosterAddress(
            Random rand,
            int? id,
            int i,
            Community community = null)
        {
            var n1 = rand.Next(100, 10000);
            var n2 = rand.Next(1, 201);
            var avOrSt = rand.Next() % 2 == 0 ? "Ave" : "St";
            var res = new CommunityUnitRosterAddress
            {
                AddressLine1 = $"{n1} {n2} {avOrSt}",
                AddressLine2 = $"Apt {i}",
                City = $"City{i}",
                PostalCode = rand.Next(10000, 99999).ToString(),
                StateCode = GetRandomUpperCaseLetter(rand) + GetRandomUpperCaseLetter(rand),
                Community = GetItem_IfNull_GetRandomItem(
                    community,
                    () => GetRandom_Community(rand, null, i))
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion CommunityUnitRosterAddress

        #region SoftwareVendor

        public static SoftwareVendor GetRandom_SoftwareVendor(
            Random rand,
            int? id,
            int i)
        {
            var res = new SoftwareVendor
            {
                Description = $"SoftwareVendor{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion SoftwareVendor

        #region SoftwareVendor

        public static CommunityImage GetRandom_CommunityImage(
            Random rand,
            int? id,
            int i,
            IReadOnlyList<ImageType> imageTypes = null)
        {
            var bytes = new byte[rand.Next(1, 100)];
            rand.NextBytes(bytes);
            var res = new CommunityImage
            {
                Content = bytes,
                ImageType = GetRandomItemArray_IfNull_GetRandomItem(
                    rand,
                    imageTypes,
                    () => GetRandom_ImageType(rand, null, rand.Next(1, 1000)))
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion SoftwareVendor

        #region ImageType

        public static ImageType GetRandom_ImageType(
            Random rand,
            int? id,
            int i)
        {
            var res = new ImageType
            {
                Name = $"ImageType{i}"
            };
            AddId(rand, id, ref res);
            return res;
        }

        #endregion ImageType

        #region CoverageRequirement

        public static CoverageRequirement GetRandom_CoverageRequirement(
            Random rand,
            int? id,
            LeaseResident leaseResident = null)
        {
            var res = new CoverageRequirement
            {
                ProductId = (int)Model.Enums.Product.PointOfLease,
                CreatedOnUtc = DateTime.UtcNow,
                LeaseResident = GetItem_IfNull_GetRandomItem(
                    leaseResident,
                    () => GetRandom_LeaseResident(rand, null))
            };

            res.Links = Enumerable.Range(1, 1).Select(item =>
                GetRandom_Link(rand, null, res, res.LeaseResident, lease: res.LeaseResident.Lease)).ToList();

            //var completed = rand.Next() % 2 != 0;
            //if (completed)
            //{
            //    res.CompletedDateOnUtc = GetRandomDate(rand);
            //    var declined = rand.Next() % 2 != 0;
            //    if (!declined)
            //    {
            //        res.CompletionMethod = rand.Next() % 3 == 0
            //            ? CoverageRequirementCompletionMethod.PurchasedExternalProduct
            //            : CoverageRequirementCompletionMethod.PurchasedInternalProduct;
            //    }
            //    else
            //    {
            //        res.CompletionMethod = CoverageRequirementCompletionMethod.DeclinedProduct;
            //    }
            //}
            AddId(rand, id, ref res);
            return res;
        }

        #endregion CoverageRequirement

        #region Link

        public static Link GetRandom_Link(
            Random rand,
            int? id,
            CoverageRequirement coverageRequirement = null,
            LeaseResident leaseResident = null,
            IReadOnlyList<Resident> residents = null,
            Model.Lease lease = null,
            ProductItem productItem = null)
        {
            var expired = rand.Next() % 7 == 0;
            var accessed = rand.Next() % 3 == 0;
            var res = new Link
            {
                CoverageRequirement = GetItem_IfNull_GetRandomItem(
                    coverageRequirement,
                    () => GetRandom_CoverageRequirement(rand, id)),
                CreatedOnUtc = DateTime.UtcNow
            };
            if (expired)
                res.ExpiredAtDateOnUtc = DateTime.UtcNow;
            if (accessed)
                res.LatestAccessedDateOnUtc = DateTime.UtcNow;
            AddId(rand, id, ref res);
            return res;
        }

        #endregion Link

        #region UtilsFunctions

        public static T GetRandomItemArray<T>(Random rand, IReadOnlyList<T> items) where T : class =>
            items[rand.Next(0, items.Count)];

        public static string GetRandomUpperCaseLetter(Random rand) =>
            ((char)('A' + rand.Next(0, 26))).ToString();

        public static DateTime GetRandomDate(Random rand) => DateTime.Now.AddDays(rand.Next(1, 50)).ToUniversalTime();

        public static DateTime GetRandomDatePass(Random rand, int max = 60) =>
            DateTime.Now.AddDays(rand.Next(1, max) * -1);

        private static T GetRandomItemArray_IfNull_GetRandomItem<T>(
            Random rand,
            IReadOnlyList<T> list,
            Func<T> getRandomItemFunc) where T : class =>
            list != null
                ? GetRandomItemArray(rand, list)
                : getRandomItemFunc();

        private static T GetItem_IfNull_GetRandomItem<T>(
            T item,
            Func<T> getRandomItemFunc) where T : class =>
            item ?? getRandomItemFunc();

        /// <summary>
        /// Setup the id of the entity. Se id param for doc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rand"></param>
        /// <param name="id">null for generate one, 0 for do nothing, int value for set it up as the id</param>
        /// <param name="entity"></param>
        private static void AddId<T>(Random rand, int? id, ref T entity) where T : IHasId
        {
            switch (id)
            {
                case null:
                    entity.Id = rand.Next(1, 1000);
                    break;

                case 0:
                    break;

                default:
                    entity.Id = id.Value;
                    break;
            }
        }

        public static string GetFirstname(Random rand)
        {
            var male = new[]
            {
                "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Charles", "Thomas",
                "Christopher", "Daniel", "Matthew", "George", "Donald", "Anthony", "Paul", "Mark", "Edward", "Steven",
                "Kenneth", "Andrew", "Brian", "Joshua", "Kevin", "Ronald", "Timothy", "Jason", "Jeffrey", "Frank",
                "Gary", "Ryan", "Nicholas", "Eric", "Stephen", "Jacob", "Larry", "Jonathan", "Scott", "Raymond",
                "Justin", "Brandon", "Gregory", "Samuel", "Benjamin", "Patrick", "Jack", "Henry", "Walter", "Dennis",
                "Jerry", "Alexander", "Peter", "Tyler", "Douglas", "Harold", "Aaron", "Jose", "Adam", "Arthur",
                "Zachary", "Carl", "Nathan", "Albert", "Kyle", "Lawrence", "Joe", "Willie", "Gerald", "Roger",
                "Keith", "Jeremy", "Terry", "Harry", "Ralph", "Sean", "Jesse", "Roy", "Louis", "Billy", "Austin",
                "Bruce", "Eugene", "Christian", "Bryan", "Wayne", "Russell", "Howard", "Fred", "Ethan", "Jordan",
                "Philip", "Alan", "Juan", "Randy", "Vincent", "Bobby", "Dylan", "Johnny", "Phillip", "Victor",
                "Clarence", "Ernest", "Martin", "Craig", "Stanley", "Shawn", "Travis", "Bradley", "Leonard", "Earl",
                "Gabriel", "Jimmy", "Francis", "Todd", "Noah", "Danny", "Dale", "Cody", "Carlos", "Allen", "Frederick",
                "Logan", "Curtis", "Alex", "Joel", "Luis", "Norman", "Marvin", "Glenn", "Tony", "Nathaniel", "Rodney",
                "Melvin", "Alfred", "Steve", "Cameron", "Chad", "Edwin", "Caleb", "Evan", "Antonio", "Lee", "Herbert",
                "Jeffery", "Isaac", "Derek", "Ricky", "Marcus", "Theodore", "Elijah", "Luke", "Jesus", "Eddie",
                "Troy", "Mike", "Dustin", "Ray", "Adrian", "Bernard", "Leroy", "Angel", "Randall", "Wesley", "Ian",
                "Jared", "Mason", "Hunter", "Calvin", "Oscar", "Clifford", "Jay", "Shane", "Ronnie", "Barry", "Lucas",
                "Corey", "Manuel", "Leo", "Tommy", "Warren", "Jackson", "Isaiah", "Connor", "Don", "Dean", "Jon",
                "Julian", "Miguel", "Bill", "Lloyd", "Charlie", "Mitchell", "Leon", "Jerome", "Darrell", "Jeremiah",
                "Alvin", "Brett", "Seth", "Floyd", "Jim", "Blake", "Micheal", "Gordon", "Trevor", "Lewis", "Erik",
                "Edgar", "Vernon", "Devin", "Gavin", "Jayden", "Chris", "Clyde", "Tom", "Derrick", "Mario", "Brent",
                "Marc", "Herman", "Chase", "Dominic", "Ricardo", "Franklin", "Maurice", "Max", "Aiden", "Owen",
                "Lester", "Gilbert", "Elmer", "Gene", "Francisco", "Glen", "Cory", "Garrett", "Clayton", "Sam",
                "Jorge", "Chester", "Alejandro", "Jeff", "Harvey", "Milton", "Cole", "Ivan", "Andre", "Duane", "Landon"
            };
            var female = new[]
            {
                "Mary", "Emma", "Elizabeth", "Minnie", "Margaret", "Ida", "Alice", "Bertha", "Sarah", "Annie", "Clara",
                "Ella", "Florence", "Cora", "Martha", "Laura", "Nellie", "Grace", "Carrie", "Maude", "Mabel", "Bessie",
                "Jennie", "Gertrude", "Julia", "Hattie", "Edith", "Mattie", "Rose", "Catherine", "Lillian", "Ada",
                "Lillie", "Helen", "Jessie", "Louise", "Ethel", "Lula", "Myrtle", "Eva", "Frances", "Lena", "Lucy",
                "Edna", "Maggie", "Pearl", "Daisy", "Fannie", "Josephine", "Dora", "Rosa", "Katherine", "Agnes",
                "Marie", "Nora", "May", "Mamie", "Blanche", "Stella", "Ellen", "Nancy", "Effie", "Sallie", "Nettie",
                "Della", "Lizzie", "Flora", "Susie", "Maud", "Mae", "Etta", "Harriet", "Sadie", "Caroline", "Katie",
                "Lydia", "Elsie", "Kate", "Susan", "Mollie", "Alma", "Addie", "Georgia", "Eliza", "Lulu", "Nannie",
                "Lottie", "Amanda", "Belle", "Charlotte", "Rebecca", "Ruth", "Viola", "Olive", "Amelia", "Hannah",
                "Jane", "Virginia", "Emily", "Matilda", "Irene", "Kathryn", "Esther", "Willie", "Henrietta", "Ollie",
                "Amy", "Rachel", "Sara", "Estella", "Theresa", "Augusta", "Ora", "Pauline", "Josie", "Lola", "Sophia",
                "Leona", "Anne", "Mildred", "Ann", "Beulah", "Callie", "Lou", "Delia", "Eleanor", "Barbara", "Iva",
                "Louisa", "Maria", "Mayme", "Evelyn", "Estelle", "Nina", "Betty", "Marion", "Bettie", "Dorothy",
                "Luella", "Inez", "Lela", "Rosie", "Allie", "Millie", "Janie", "Cornelia", "Victoria", "Ruby",
                "Winifred", "Alta", "Celia", "Christine", "Beatrice", "Birdie", "Harriett", "Mable", "Myra", "Sophie",
                "Tillie", "Isabel", "Sylvia", "Carolyn", "Isabelle", "Leila", "Sally", "Ina", "Essie", "Bertie", "Nell",
                "Alberta", "Katharine", "Lora", "Rena", "Mina", "Rhoda", "Mathilda", "Abbie", "Eula", "Dollie",
                "Hettie", "Eunice", "Fanny", "Ola", "Lenora", "Adelaide", "Christina", "Lelia", "Nelle", "Sue",
                "Johanna", "Lilly", "Lucinda", "Minerva", "Lettie", "Roxie", "Cynthia", "Helena", "Hilda", "Hulda",
                "Bernice", "Genevieve", "Jean", "Cordelia", "Marian", "Francis", "Jeanette", "Adeline", "Gussie",
                "Leah", "Lois", "Lura", "Mittie", "Hallie", "Isabella", "Olga", "Phoebe", "Teresa", "Hester", "Lida",
                "Lina", "Winnie", "Claudia", "Marguerite", "Vera", "Cecelia", "Bess", "Emilie", "John", "Rosetta",
                "Verna", "Myrtie", "Cecilia", "Elva", "Olivia", "Ophelia", "Georgie", "Elnora", "Violet", "Adele",
                "Lily", "Linnie", "Loretta", "Madge", "Polly", "Virgie", "Eugenia", "Lucile", "Lucille", "Mabelle",
                "Rosalie"
            };

            return rand.Next() % 2 == 0
                ? male[rand.Next(0, male.Length)]
                : female[rand.Next(0, female.Length)];
        }

        public static string GetLastname(Random rand)
        {
            var lastNames = new[]
            {
                "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor",
                "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez",
                "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez",
                "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Gonzalez", "Nelson", "Carter",
                "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards",
                "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy",
                "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray",
                "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes",
                "Ross", "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores",
                "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin",
                "Diaz", "Hayes", "Myers", "Ford", "Hamilton", "Graham", "Sullivan", "Wallace", "Woods", "Cole",
                "West", "Jordan", "Owens", "Reynolds", "Fisher", "Ellis", "Harrison", "Gibson", "McDonald", "Cruz",
                "Marshall", "Ortiz", "Gomez", "Murray", "Freeman", "Wells", "Webb", "Simpson", "Stevens", "Tucker",
                "Porter", "Hunter", "Hicks", "Crawford", "Henry", "Boyd", "Mason", "Morales", "Kennedy", "Warren",
                "Dixon", "Ramos", "Reyes", "Burns", "Gordon", "Shaw", "Holmes", "Rice", "Robertson", "Hunt", "Black",
                "Daniels", "Palmer", "Mills", "Nichols", "Grant", "Knight", "Ferguson", "Rose", "Stone", "Hawkins",
                "Dunn", "Perkins", "Hudson", "Spencer", "Gardner", "Stephens", "Payne", "Pierce", "Berry", "Matthews",
                "Arnold", "Wagner", "Willis", "Ray", "Watkins", "Olson", "Carroll", "Duncan", "Snyder", "Hart",
                "Cunningham", "Bradley", "Lane", "Andrews", "Ruiz", "Harper", "Fox", "Riley", "Armstrong",
                "Carpenter", "Weaver", "Greene", "Lawrence", "Elliott", "Chavez", "Sims", "Austin", "Peters",
                "Kelley", "Franklin", "Lawson", "Fields", "Gutierrez", "Ryan", "Schmidt", "Carr", "Vasquez",
                "Castillo", "Wheeler", "Chapman", "Oliver", "Montgomery", "Richards", "Williamson", "Johnston",
                "Banks", "Meyer", "Bishop", "McCoy", "Howell", "Alvarez", "Morrison", "Hansen", "Fernandez", "Garza",
                "Harvey", "Little", "Burton", "Stanley", "Nguyen", "George", "Jacobs", "Reid", "Kim", "Fuller",
                "Lynch", "Dean", "Gilbert", "Garrett", "Romero", "Welch", "Larson", "Frazier", "Burke", "Hanson",
                "Day", "Mendoza", "Moreno", "Bowman", "Medina", "Fowler", "Brewer", "Hoffman", "Carlson", "Silva",
                "Pearson", "Holland", "Douglas", "Fleming", "Jensen", "Vargas", "Byrd", "Davidson", "Hopkins", "May",
                "Terry", "Herrera", "Wade", "Soto", "Walters", "Curtis", "Neal", "Caldwell", "Lowe", "Jennings",
                "Barnett", "Graves", "Jimenez", "Horton", "Shelton", "Barrett", "Obrien", "Castro", "Sutton",
                "Gregory", "McKinney", "Lucas", "Miles", "Craig", "Rodriquez", "Chambers", "Holt", "Lambert",
                "Fletcher", "Watts", "Bates", "Hale", "Rhodes", "Pena", "Beck", "Newman", "Haynes", "McDaniel",
                "Mendez", "Bush", "Vaughn", "Parks", "Dawson", "Santiago", "Norris", "Hardy", "Love", "Steele",
                "Curry", "Powers", "Schultz", "Barker", "Guzman", "Page", "Munoz", "Ball", "Keller", "Chandler",
                "Weber", "Leonard", "Walsh", "Lyons", "Ramsey", "Wolfe", "Schneider", "Mullins", "Benson", "Sharp",
                "Bowen", "Daniel", "Barber", "Cummings", "Hines", "Baldwin", "Griffith", "Valdez", "Hubbard",
                "Salazar", "Reeves", "Warner", "Stevenson", "Burgess", "Santos", "Tate", "Cross", "Garner", "Mann",
                "Mack", "Moss", "Thornton", "Dennis", "McGee", "Farmer", "Delgado", "Aguilar", "Vega", "Glover",
                "Manning", "Cohen", "Harmon", "Rodgers", "Robbins", "Newton", "Todd", "Blair", "Higgins", "Ingram",
                "Reese", "Cannon", "Strickland", "Townsend", "Potter", "Goodwin", "Walton", "Rowe", "Hampton",
                "Ortega", "Patton", "Swanson", "Joseph", "Francis", "Goodman", "Maldonado", "Yates", "Becker",
                "Erickson", "Hodges", "Rios", "Conner", "Adkins", "Webster", "Norman", "Malone", "Hammond", "Flowers",
                "Cobb", "Moody", "Quinn", "Blake", "Maxwell", "Pope", "Floyd", "Osborne", "Paul", "McCarthy",
                "Guerrero", "Lindsey", "Estrada", "Sandoval", "Gibbs", "Tyler", "Gross", "Fitzgerald", "Stokes",
                "Doyle", "Sherman", "Saunders", "Wise", "Colon", "Gill", "Alvarado", "Greer", "Padilla", "Simon",
                "Waters", "Nunez", "Ballard", "Schwartz", "McBride", "Houston", "Christensen", "Klein", "Pratt",
                "Briggs", "Parsons", "McLaughlin", "Zimmerman", "French", "Buchanan", "Moran", "Copeland", "Roy",
                "Pittman", "Brady", "McCormick", "Holloway", "Brock", "Poole", "Frank", "Logan", "Owen", "Bass",
                "Marsh", "Drake", "Wong", "Jefferson", "Park", "Morton", "Abbott", "Sparks", "Patrick", "Norton",
                "Huff", "Clayton", "Massey", "Lloyd", "Figueroa", "Carson", "Bowers", "Roberson", "Barton", "Tran",
                "Lamb", "Harrington", "Casey", "Boone", "Cortez", "Clarke", "Mathis", "Singleton", "Wilkins", "Cain",
                "Bryan", "Underwood", "Hogan", "McKenzie", "Collier", "Luna", "Phelps", "McGuire", "Allison",
                "Bridges", "Wilkerson", "Nash", "Summers", "Atkins"
            };

            return lastNames[rand.Next(0, lastNames.Length)];
        }

        #endregion UtilsFunctions
    }
}