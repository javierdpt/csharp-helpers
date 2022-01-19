using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using .Lease.Api.Infrastructure.Exceptions;
using .Lease.Data;
using .Lease.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IHasId = .Lease.Data.Models.IHasId;

namespace .Lease.Api.Controllers.Base
{
    /// <inheritdoc />
    /// <summary>
    /// Base RESTFull CRURD api controller
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityDto"></typeparam>
    /// <typeparam name="TQueryFilter"></typeparam>
    public abstract class GenericCrudApiController<TEntity, TEntityDto, TQueryFilter> : GenericReadApiController<TEntity, TEntityDto, TQueryFilter>
        where TEntity : BaseIdEntity
        where TEntityDto : IHasId
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructor with the required dependencies
        /// </summary>
        /// <param name="dbContextEf6"></param>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="dbContext"></param>
        protected GenericCrudApiController(ILeaseDbContext dbContext, ILeaseDbContextEf6 dbContextEf6, ILogger logger, IMapper mapper)
            : base(dbContext, dbContextEf6, logger, mapper) { }

        // PUT: api/TEntities/5
        /// <summary>Update an TEntity</summary>
        /// <param name="id">Entity Id</param>
        /// <param name="entity">Etity fields</param>
        /// <returns>ActionResult of TEntityDto type</returns>
        /// <response code="200">Returns update TEntity</response>
        /// <response code="404">If the entity doesn't exist</response>
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> PutTEntity(int id, [FromBody] TEntity entity)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException($"Validation errors: {JsonConvert.SerializeObject(ModelState.SelectMany(e => e.Value.Errors))}");

            if (id != entity.Id)
                throw new NotFoundException($"Could not find entity of type: {typeof(TEntity)} with id: {id}.");

            Update(entity);

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EntityExists(id))
                    throw new NotFoundException("Concurrency issue.");
                throw;
            }

            return Ok(Mapper.Map<TEntity, TEntityDto>(entity));
        }

        // POST: api/TEntities
        /// <summary>Create and TEntity</summary>
        /// <param name="entity">Etity fields</param>
        /// <returns>ActionResult of TEntityDto type</returns>
        /// <response code="200">Returns created TEntity</response>
        [HttpPost]
        public virtual async Task<IActionResult> PostTEntity([FromBody] TEntity entity)
        {
            if (!ModelState.IsValid)
            {
                throw new BadRequestException($"Validation errors: {JsonConvert.SerializeObject(ModelState.SelectMany(e => e.Value.Errors))}");
            }

            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();

            return Ok(Mapper.Map<TEntity, TEntityDto>(entity));
        }

        // DELETE: api/TEntities/5
        /// <summary>Delete an TEntity by id</summary>
        /// <param name="id">Entity Id</param>
        /// <returns>ActionResult of TEntity type</returns>
        /// <response code="200">Returns deleted TEntity</response>
        /// <response code="404">If the entity is not found</response>
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteTEntity(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
            {
                throw new NotFoundException("Entity not found");
            }

            Delete(entity);
            await DbContext.SaveChangesAsync();

            return Ok(Mapper.Map<TEntity, TEntityDto>(entity));
        }

        /// <summary>Check if an entity with id='id' exist</summary>
        /// <param name="id"></param>
        /// <returns>True if the entity exist false otherwise</returns>
        private async Task<bool> EntityExists(int id)
        {
            return await DbSet.FindAsync(id) != null;
        }
    }
}