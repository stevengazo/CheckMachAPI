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
    public class MaintenancesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaintenancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Maintenances
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Maintenance>>> GetMaintenances()
        {
            return await _context.Maintenances.ToListAsync();
        }

        // GET: api/Machines/searchByMaintenance?type=Preventive&status=Completed&author=John
        [HttpGet("searchByMaintenance")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachinesByMaintenance(
            [FromQuery] string? type,
            [FromQuery] string? status,
            [FromQuery] string? author)
        {
            var query = _context.Machines.AsQueryable();

            // Filtramos las máquinas que tengan algún mantenimiento que cumpla con los criterios
            if (!string.IsNullOrWhiteSpace(type) || !string.IsNullOrWhiteSpace(status) || !string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(m => m.Maintenances.Any(mt =>
                    (string.IsNullOrWhiteSpace(type) || mt.Type != null && mt.Type.Contains(type)) &&
                    (string.IsNullOrWhiteSpace(status) || mt.Status != null && mt.Status.Contains(status)) &&
                    (string.IsNullOrWhiteSpace(author) || mt.Author != null && mt.Author.Contains(author))
                ));
            }

            var result = await query.Include(m => m.Maintenances).ToListAsync();

            if (result.Count == 0)
                return NotFound("No se encontraron máquinas con mantenimientos que cumplan los criterios especificados.");

            return Ok(result);
        }


        // GET: api/Maintenances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Maintenance>> GetMaintenance(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            return maintenance;
        }

        // PUT: api/Maintenances/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMaintenance(int id, Maintenance maintenance)
        {
            if (id != maintenance.MaintenanceId)
            {
                return BadRequest();
            }

            _context.Entry(maintenance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceExists(id))
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

        // POST: api/Maintenances
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Maintenance>> PostMaintenance(Maintenance maintenance)
        {
            _context.Maintenances.Add(maintenance);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaintenance", new { id = maintenance.MaintenanceId }, maintenance);
        }

        // DELETE: api/Maintenances/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenance(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaintenanceExists(int id)
        {
            return _context.Maintenances.Any(e => e.MaintenanceId == id);
        }
    }
}
