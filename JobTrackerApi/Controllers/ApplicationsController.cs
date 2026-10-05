using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTrackerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace JobTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        //private readonly List<Application> _applications;

        public ApplicationsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Applications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application>>> GetApplications()
        {
            return _context.Applications;
        }
        // GET: api/Applications/5
        [HttpGet("{id}")]
        // public IActionResult GetById(int id)
        // {
        //     var app = _applications.FirstOrDefault(a => a.Id == id);
        //     return app is null? NotFound() : Ok(app);
        // }
        public async Task<ActionResult<Application>> GetApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);

            if (application == null)
            {
                return NotFound();
            }

            return application;
        }

        // PUT: api/Applications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        // public IActionResult PutApplication(int id, Application updatedApp)
        // {
        //     if (id != updatedApp.Id)
        //     {
        //         return BadRequest();
        //     }
        //     var idx = _applications.FindIndex(app => app.Id == id);
        //     if (idx == -1)
        //     {
        //         return NotFound();
        //     }
        //     _applications[idx] = updatedApp;
        //     return Ok();
        // }
        public async Task<IActionResult> PutApplication(int id, Application application)
        {
            if (id != application.Id)
            {
                return BadRequest();
            }

            _context.Entry(application).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Applications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        // public IActionResult Create(Application app)
        // {
        //     _applications.Add(app);
        //     return CreatedAtAction(nameof(GetById), new {id = app.Id});
        // }
        
        public async Task<ActionResult<Application>> PostApplication(Application application)
        {
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApplication", new { id = application.Id }, application);
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
        // public IActionResult DeleteApplication(int id)
        // {
        //     var idx = _applications.FindIndex(app => app.Id == id);
        //     if (idx == -1)
        //     {
        //         return NotFound();
        //     }
        //     _applications.RemoveAt(idx);
        //     return NoContent();
        // }
        private bool ApplicationExists(int id)
        {
            return _context.Applications.Any(e => e.Id == id);
        }
    }
}
