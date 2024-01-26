using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadisDemo.Contextes;
using RadisDemo.Models;
using RadisDemo.Services;

namespace RadisDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;

        public DriversController(ILogger<DriversController> logger, ICacheService cacheService, AppDbContext context)
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }

        [HttpGet("drivers")]
        public async Task<ActionResult> Get()
        {
            // check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("Drivers");

            if (cacheData != null && cacheData.Count() > 0)
                return Ok(cacheData);

            cacheData = await _context.Drivers.ToListAsync();

            // set expiry time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            return Ok(cacheData);
        }

        [HttpPost("AddDrivers")]
        public async Task<ActionResult> Post(Driver value)
        {
            var addObj = await _context.Drivers.AddAsync(value);

            // set in the cache
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<Driver>($"driver{value.Id}", addObj.Entity, expiryTime);

            await _context.SaveChangesAsync();

            return Ok(addObj.Entity);
        }

        [HttpDelete("DeleteDriver")]
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            if (exist != null)
            {
                _context.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();
        }
    }
}
