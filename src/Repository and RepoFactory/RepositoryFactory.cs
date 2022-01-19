using System;
using System.Collections.Generic;
using System.Threading;

namespace .CommunityData.Data.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly Dictionary<Type, object> _repositories;
        private readonly IMfhDbContext _mfhDbContext;
        private readonly CancellationToken _cancellationToken;

        public RepositoryFactory(IMfhDbContext mfhDbContext, CancellationToken cancellationToken)
        {
            _repositories = new Dictionary<Type, object>();
            _mfhDbContext = mfhDbContext;
            _cancellationToken = cancellationToken;
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                _repositories[typeof(T)] = new Repository<T>(_mfhDbContext, _cancellationToken);
            }

            return (IRepository<T>)_repositories[typeof(T)];
        }

        public TIRepo GetCustomRepository<TIRepo, TRepo, TEntity>() 
            where TEntity: class
            where TRepo : class, IRepository<TEntity>
            where TIRepo : IRepository<TEntity>
        {
            if (!_repositories.ContainsKey(typeof(TRepo)))
            {
                _repositories[typeof(TRepo)] = Activator.CreateInstance(typeof(TRepo), _mfhDbContext, _cancellationToken);
            }

            return (TIRepo) _repositories[typeof(TRepo)];
        }
    }
}