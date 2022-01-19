using AutoMapper;
using Lease.Api.Infrastructure.Constants;
using Lease.Data;
using Lease.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace .Lease.Api.Controllers.Base
{
    /// <summary>BaseController with dependencies</summary>
    [Authorize(Policy = PoliciesConstants.ScopeLeaseApi)]
    [Route("api/v1/[controller]")]
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// App Context using
        /// </summary>
        protected ILeaseDbContext DbContext;

        /// <summary>
        ///
        /// </summary>
        protected ILeaseDbContextEf6 DbContextEf6;

        /// <summary>
        /// App Logger
        /// </summary>
        protected ILogger Logger;

        /// <summary>
        /// AutoMapper instance
        /// </summary>
        protected IMapper Mapper;

        /// <summary>
        /// Constructor for BaseController with the required dependencies
        /// </summary>
        /// <param name="dbContextEf6"></param>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="dbContext"></param>
        protected BaseController(ILeaseDbContext dbContext, ILeaseDbContextEf6 dbContextEf6, ILogger logger, IMapper mapper)
        {
            DbContext = dbContext;
            DbContextEf6 = dbContextEf6;
            Logger = logger;
            Mapper = mapper;
        }

        /// <summary>
        /// Update and Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(T entity) where T : BaseEntity
        {
            var dbSet = DbContext.Set<T>();
            var dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbEntityEntry.State = EntityState.Modified;
        }

        /// <summary>
        /// Delete and Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Delete<T>(T entity) where T : BaseEntity
        {
            var dbSet = DbContext.Set<T>();
            var dbEntityEntry = DbContext.Entry(entity);
            if (dbEntityEntry.State != EntityState.Deleted)
            {
                dbEntityEntry.State = EntityState.Deleted;
            }
            else
            {
                dbSet.Attach(entity);
                dbSet.Remove(entity);
            }
        }

        /// <summary>
        /// Base controller dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbContext.Dispose();
                DbContextEf6.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}