### Create custom repository

    // Define the interface
    public interface ICommunityRepository : IRepository<Community>
    {
        Task<List<CommunityNotification>> GetCommunityNotification(int communityId);
    }

    // Define the repo
    public class CommunityRepository : Repository<Community>, ICommunityRepository
    {
        public CommunityRepository(IMfhDbContext mfhDbContext, CancellationToken cancellationToken) : base(mfhDbContext, cancellationToken)
        {
        }

        public Task<List<CommunityNotification>> GetCommunityNotification(int communityId)
        {
            var query =
                from c in DbSet
                from n in c.Notifications
                where c.Id == communityId
                select n;
            return query.ToListAsync();
        }
    }

    // Call the repo
    var customRepo = _repositoryFactory.GetCustomRepository<ICommunityRepository, CommunityRepository, Community>();
    var data = await customRepo.GetCommunityNotification(12345);




