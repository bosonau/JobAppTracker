using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTrackerApi.Models;
using JobTrackerApi.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace JobTrackerApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IDistributedCache _cache;
        private readonly ApplicationSearchService _search;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(ApplicationContext context, ILogger<ApplicationsController> logger, IDistributedCache cache, ApplicationSearchService search)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _search = search;
        }

        private ApplicationDoc ToDoc(Application a) => new () {
            Id = a.Id,
            UserId = UserId,
            Company = a.CompanyName,
            Title = a.RoleTitle,
            Status = a.Status.ToString(),
            Notes = a.Notes,
            AppliedDate = a.DateApplied
        };

        private string UserId => User.FindFirstValue(ClaimTypes.Name) ?? "anon";
        private async Task<string> GetVersionAsync()
        {
            try { return await _cache.GetStringAsync($"apps:ver:{UserId}") ?? "0"; }
            catch (Exception ex) { _logger.LogWarning(ex, "Redis version read failed"); return "0"; }
        }
        private async Task BumpVersionAsync()
        {
            try { await _cache.SetStringAsync($"apps:ver:{UserId}", Guid.NewGuid().ToString("N")); }
            catch (Exception ex) { _logger.LogWarning(ex, "Redis version bump failed"); }
        }


        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetApplications([FromQuery] string? companyName, [FromQuery] string? status)
        {
            // TODO: use redis.
            
            // set version
            var version = await GetVersionAsync();
            // set key to check for
            var key = $"apps:{UserId}:v{version}:{companyName?.ToLowerInvariant()}:{status}";
            // try to get from cache the key as cached
            // if exists, response.headers["X-Cache"] = "HIT";
            //   return Ok with deserliazed of list of ApplicationDTOs from cached
            // catch on try to log redis is down
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached is not null)
                {
                    Response.Headers["X-Cache"] = "HIT";
                    return Ok(JsonSerializer.Deserialize<List<ApplicationResponse>>(cached));
                }
            }
            catch(Exception ex)
            { 
                _logger.LogWarning(ex, "Redis read failed, falling back to db"); 
            }

            // cache did not read (or null)            
            IQueryable<Application> query = _context.Applications;

            //daisy chain based on parameters
            if(!string.IsNullOrWhiteSpace(companyName))
            { query = query.Where(a => a.CompanyName.ToLower().Contains(companyName.Trim().ToLower()));}
            
            if(!string.IsNullOrWhiteSpace(status))
            { query = query.Where(a => a.Status.Equals(status));}
            
            List<ApplicationResponse> results = await query.OrderBy(a => a.CompanyName)
                .Select(a => a.ToResponse())
                .ToListAsync();

            //try to write to redis
            try
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(results), 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });
            }
            catch(Exception ex)
            { 
                _logger.LogWarning(ex, "Redis write failed"); 
            }
            Response.Headers["X-Cache"] = "MISS";
            return Ok(results);
        }
        // GET: api/Applications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResponse>> GetApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }
            return application.ToResponse();
        }

        // PUT: api/Applications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplication(int id, UpdateApplicationRequest request)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            request.ApplyTo(application);
            await _context.SaveChangesAsync();
            await BumpVersionAsync();
            await _search.IndexAsync(ToDoc(application));
            return NoContent();
        }

        // POST: api/Applications
        [HttpPost]        
        public async Task<ActionResult<ApplicationResponse>> PostApplication(CreateApplicationRequest request)
        {
            var application = request.ToEntity();
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            await BumpVersionAsync();
            await _search.IndexAsync(ToDoc(application));
            return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, application.ToResponse());
        }

        // DELETE: api/Applications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            await BumpVersionAsync();
            await _search.DeleteAsync(id);

            return NoContent();
        }

        [HttpPost("reindex")]
        public async Task<IActionResult> Reindex([FromServices] IWebHostEnvironment env)
        {
            if (!env.IsDevelopment()) return NotFound();
            var docs = await _context.Applications
                .AsNoTracking()
                .Select(a => new ApplicationDoc
                {
                    Id = a.Id,
                    UserId = UserId.ToString(),
                    Company = a.CompanyName,
                    Title = a.RoleTitle,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    AppliedDate = a.DateApplied
                })
                .ToListAsync();
            await _search.ReindexAllAsync(docs);
            return Ok(new {indexed = docs.Count});                
        }
        [HttpGet("search")]
        public async Task<ActionResult<List<ApplicationDoc>>> SearchText([FromQuery] string q)
        {
            if (String.IsNullOrWhiteSpace(q)) return Ok(new List<ApplicationDoc>());
            try { 
                return Ok(await _search.SearchAsync(UserId, q, null));}
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Search Unavailable");
                return StatusCode(503, "Search is temporary Unavailable");
            }
        }   
    }
}
