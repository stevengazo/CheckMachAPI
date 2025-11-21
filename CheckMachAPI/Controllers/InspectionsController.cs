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
    public class InspectionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InspectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Inspections
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inspection>>> GetInspections()
        {
            return await _context.Inspections.ToListAsync();
        }

        // GET: api/Inspections/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Inspection>>> SearchInspections(
            [FromQuery] string? description,
            [FromQuery] DateTime? date,
            [FromQuery] int? machineId,
            [FromQuery] bool? pass)
        {
            var query = _context.Inspections
                .Include(i => i.Machine)
                .Include(i => i.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(i => i.Description.Contains(description));

            if (date.HasValue)
                query = query.Where(i => i.InspectionDate.Date == date.Value.Date);

            if (machineId.HasValue)
                query = query.Where(i => i.MachineId == machineId.Value);

            if (pass.HasValue)
                query = query.Where(i => i.Pass == pass.Value);

            var results = await query.ToListAsync();

            if (!results.Any())
                return NotFound(new { Message = "No se encontraron inspecciones con los criterios especificados." });

            return Ok(results);
        }


        // GET: api/Inspections/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inspection>> GetInspection(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);

            if (inspection == null)
            {
                return NotFound();
            }

            return inspection;
        }

        // PUT: api/Inspections/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInspection(int id, Inspection inspection)
        {
            if (id != inspection.InspectionId)
            {
                return BadRequest();
            }

            _context.Entry(inspection).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InspectionExists(id))
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

        // POST: api/Inspections
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Inspection>> PostInspection(Inspection inspection)
        {
            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInspection", new { id = inspection.InspectionId }, inspection);
        }

        // DELETE: api/Inspections/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspection(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }

            _context.Inspections.Remove(inspection);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.InspectionId == id);
        }
    }
}
