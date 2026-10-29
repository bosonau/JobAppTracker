using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTrackerApi.Models;
using JobTrackerApi.Dtos;

namespace JobTrackerApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ApplicationsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetApplications([FromQuery] string? companyName)
        {
            IQueryable<Application> query = _context.Applications;
            if(!string.IsNullOrWhiteSpace(companyName))
            {
                var term = companyName.Trim().ToLower();
                query = query.Where(a => a.CompanyName.ToLower().Contains(term));
            }
            return await query.OrderBy(a => a.CompanyName)
                .Select(a => a.ToResponse())
                .ToListAsync();
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
            return NoContent();
        }

        // POST: api/Applications
        [HttpPost]        
        public async Task<ActionResult<ApplicationResponse>> PostApplication(CreateApplicationRequest request)
        {
            var application = request.ToEntity();
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
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

            return NoContent();
        }
    }
}
