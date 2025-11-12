using CheckMachAPI.Data;
using CheckMachAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMachAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TimePeerProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimePeerProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TimePeerProjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimePeerProject>>> GetTimePeerProjects()
        {
            return await _context.TimePeerProjects.ToListAsync();
        }

        // GET: api/Machines/searchFullNested
        [HttpGet("searchFullNested")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachinesFullNested(
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

            // Filtrar por propiedades de la máquina
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(m => m.Name != null && m.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(m => m.Model != null && m.Model.Contains(model));
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(m => m.Location != null && m.Location.Contains(location));

            // Filtrar por mantenimientos
            if (!string.IsNullOrWhiteSpace(maintenanceType) ||
                !string.IsNullOrWhiteSpace(maintenanceStatus) ||
                !string.IsNullOrWhiteSpace(maintenanceAuthor))
            {
                query = query.Where(m => m.Maintenances.Any(mt =>
                    (string.IsNullOrWhiteSpace(maintenanceType) || (mt.Type != null && mt.Type.Contains(maintenanceType))) &&
                    (string.IsNullOrWhiteSpace(maintenanceStatus) || (mt.Status != null && mt.Status.Contains(maintenanceStatus))) &&
                    (string.IsNullOrWhiteSpace(maintenanceAuthor) || (mt.Author != null && mt.Author.Contains(maintenanceAuthor)))
                ));
            }

            // Filtrar por proyectos vía TimePeerProject
            if (!string.IsNullOrWhiteSpace(projectName) || projectId.HasValue)
            {
                query = query.Where(m => m.Times.Any(t =>
                    (string.IsNullOrWhiteSpace(projectName) || (t.Project != null && t.Project.Name != null && t.Project.Name.Contains(projectName))) &&
                    (!projectId.HasValue || t.ProjectId == projectId)
                ));
            }

            // Filtrar por fotos
            if (!string.IsNullOrWhiteSpace(photoType))
            {
                query = query.Where(m => _context.Photos.Any(p => p.ReferenceId == m.MachineId && p.PhotoType != null && p.PhotoType.Contains(photoType)));
            }

            // Incluir relaciones completas
            query = query
                .Include(m => m.Maintenances)
                .Include(m => m.Times)
                    .ThenInclude(t => t.Project)
                .Include(m => m.Times)
                    .ThenInclude(t => t.Machine);

            // Ejecutar consulta
            var machines = await query.ToListAsync();

            // Incluir fotos manualmente en cada máquina para respuesta anidada
            foreach (var machine in machines)
            {
                var photos = await _context.Photos
                    .Where(p => p.ReferenceId == machine.MachineId &&
                                (string.IsNullOrWhiteSpace(photoType) || (p.PhotoType != null && p.PhotoType.Contains(photoType))))
                    .ToListAsync();

                // Añadimos una propiedad temporal para fotos (puede ser un DTO si quieres)
                machine.GetType().GetProperty("Photos")?.SetValue(machine, photos);
            }

            if (!machines.Any())
                return NotFound("No se encontraron máquinas que cumplan los criterios especificados.");

            return Ok(machines);
        }


        // GET: api/TimePeerProjects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TimePeerProject>> GetTimePeerProject(int id)
        {
            var timePeerProject = await _context.TimePeerProjects.FindAsync(id);

            if (timePeerProject == null)
            {
                return NotFound();
            }

            return timePeerProject;
        }

        // PUT: api/TimePeerProjects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimePeerProject(int id, TimePeerProject timePeerProject)
        {
            if (id != timePeerProject.TimePeerProjectId)
            {
                return BadRequest();
            }

            _context.Entry(timePeerProject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimePeerProjectExists(id))
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

        // POST: api/TimePeerProjects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TimePeerProject>> PostTimePeerProject(TimePeerProject timePeerProject)
        {
            _context.TimePeerProjects.Add(timePeerProject);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTimePeerProject", new { id = timePeerProject.TimePeerProjectId }, timePeerProject);
        }

        // DELETE: api/TimePeerProjects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimePeerProject(int id)
        {
            var timePeerProject = await _context.TimePeerProjects.FindAsync(id);
            if (timePeerProject == null)
            {
                return NotFound();
            }

            _context.TimePeerProjects.Remove(timePeerProject);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TimePeerProjectExists(int id)
        {
            return _context.TimePeerProjects.Any(e => e.TimePeerProjectId == id);
        }
    }
}
