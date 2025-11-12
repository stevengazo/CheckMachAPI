using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CheckMachAPI.Data;
using CheckMachAPI.Models;

namespace CheckMachAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Machines/searchAdvanced?name=Excavator&projectName=ProjectA&photoType=Machine
        [HttpGet("searchAdvanced")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachinesAdvanced(
            [FromQuery] string? name,
            [FromQuery] string? model,
            [FromQuery] string? location,
            [FromQuery] string? maintenanceType,
            [FromQuery] string? maintenanceStatus,
            [FromQuery] string? maintenanceAuthor,
            [FromQuery] string? photoType,
            [FromQuery] string? projectName,
            [FromQuery] int? projectId)
        {
            var query = _context.Machines.AsQueryable();

            // Filtrado por propiedades de la máquina
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(m => m.Name != null && m.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(m => m.Model != null && m.Model.Contains(model));
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(m => m.Location != null && m.Location.Contains(location));

            // Filtrado por mantenimientos
            if (!string.IsNullOrWhiteSpace(maintenanceType) || !string.IsNullOrWhiteSpace(maintenanceStatus) || !string.IsNullOrWhiteSpace(maintenanceAuthor))
            {
                query = query.Where(m => m.Maintenances.Any(mt =>
                    (string.IsNullOrWhiteSpace(maintenanceType) || (mt.Type != null && mt.Type.Contains(maintenanceType))) &&
                    (string.IsNullOrWhiteSpace(maintenanceStatus) || (mt.Status != null && mt.Status.Contains(maintenanceStatus))) &&
                    (string.IsNullOrWhiteSpace(maintenanceAuthor) || (mt.Author != null && mt.Author.Contains(maintenanceAuthor)))
                ));
            }

            // Filtrado por fotos
            if (!string.IsNullOrWhiteSpace(photoType))
            {
                query = query.Where(m => _context.Photos.Any(p => p.ReferenceId == m.MachineId && p.PhotoType != null && p.PhotoType.Contains(photoType)));
            }

            // Filtrado por proyectos a través de TimePeerProject
            if (!string.IsNullOrWhiteSpace(projectName) || projectId.HasValue)
            {
                query = query.Where(m => m.Times.Any(t =>
                    (string.IsNullOrWhiteSpace(projectName) || (t.Project != null && t.Project.Name != null && t.Project.Name.Contains(projectName))) &&
                    (!projectId.HasValue || t.ProjectId == projectId)
                ));
            }

            // Incluir mantenimientos y relaciones importantes
            query = query.Include(m => m.Maintenances)
                         .Include(m => m.Times)
                         .ThenInclude(t => t.Project);

            var result = await query.ToListAsync();

            if (!result.Any())
                return NotFound("No se encontraron máquinas que cumplan los criterios especificados.");

            return Ok(result);
        }


        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        // PUT: api/Projects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.ProjectId)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Projects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.ProjectId }, project);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
