using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using .Lease.Api.Infrastructure.Exceptions;
using .Lease.Data;
using .Lease.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IHasId = .Lease.Data.Models.IHasId;

namespace .Lease.Api.Controllers.Base
{
    /// <summary>
    /// Base READ api controller
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityDto"></typeparam>
    /// <typeparam name="TQueryFilter"></typeparam>
    public abstract class GenericReadApiController<TEntity, TEntityDto, TQueryFilter> : BaseController
        where TEntity : BaseEntity
        where TEntityDto : IHasId
    {
        /// <summary>
        /// The standard repository of IEntity
        /// </summary>
        protected readonly DbSet<TEntity> DbSet;

        /// <summary>
        /// Constructor with the required dependencies
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="dbContext"></param>
        /// <param name="dbContextEf6"></param>
        protected GenericReadApiController(ILeaseDbContext dbContext, ILeaseDbContextEf6 dbContextEf6, ILogger logger, IMapper mapper)
            : base(dbContext, dbContextEf6, logger, mapper)
        {
            DbSet = dbContext.Set<TEntity>();
        }

        // GET: api/TEntities
        /// <summary>List of TEntities filtered by TQueryFilter</summary>
        /// <param name="queryFilter">Query filter parameter</param>
        /// <returns>List of TEntityDto type</returns>
        /// <response code="200">Returns filtered TEntities</response>
        [HttpGet]
        public virtual async Task<List<TEntityDto>> GetTEntities([FromQuery] TQueryFilter queryFilter)
        {
            var entities = await EntitiesQuery(queryFilter).ToListAsync();
            var mappedEntities = Mapper.Map<List<TEntity>, List<TEntityDto>>(entities);
            return mappedEntities;
        }

        // GET: api/TEntities/5
        /// <summary>TEntity filterd by id</summary>
        /// <param name="id"></param>
        /// <returns>ActionResult of TEntityDto</returns>
        /// /// <response code="200">Returns the entity whose Id = 'id'</response>
        /// <response code="404">If the entity doesn't exist</response>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetTEntityById(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                throw new NotFoundException("Entity not found");

            return Ok(Mapper.Map<TEntity, TEntityDto>(entity));
        }

        /// <summary>Returns the FindAll base query to be used to filter the entities</summary>
        /// <param name="queryFilter"></param>
        /// <returns>IQueryable de TEntity</returns>
        protected virtual IQueryable<TEntity> EntitiesQuery(TQueryFilter queryFilter)
        {
            return DbSet;
        }
    }
}