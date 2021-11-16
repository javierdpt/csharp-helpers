namespace GS.MFH.CommunityData.Data.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>() where T : class;

        TIRepo GetCustomRepository<TIRepo, TRepo, TEntity>()
            where TEntity : class
            where TRepo : class, IRepository<TEntity>
            where TIRepo : IRepository<TEntity>;
    }
}