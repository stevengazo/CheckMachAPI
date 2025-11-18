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
    public class MachinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MachinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Machines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Machine>>> GetMachines()
        {
            return await _context.Machines.ToListAsync();
        }

        // GET: api/Machines/search?name=Excavator&model=X200&location=Warehouse
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachines(
            [FromQuery] string? name,
            [FromQuery] string? model,
            [FromQuery] string? location)
        {
            // Partimos de todos los registros
            var query = _context.Machines.AsQueryable();

            // Filtramos según los parámetros que vengan
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(m => m.Name != null && m.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(m => m.Model != null && m.Model.Contains(model));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(m => m.Location != null && m.Location.Contains(location));
            }

            var result = await query.ToListAsync();

            if (result.Count == 0)
                return NotFound("No se encontraron máquinas con los criterios especificados.");

            return Ok(result);
        }


        // GET: api/Machines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Machine>> GetMachine(int id)
        {
            var machine = await _context.Machines.FindAsync(id);

            if (machine == null)
            {
                return NotFound();
            }

            return machine;
        }

        // PUT: api/Machines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMachine(int id, Machine machine)
        {
            if (id != machine.MachineId)
            {
                return BadRequest();
            }

            _context.Entry(machine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MachineExists(id))
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

        // POST: api/Machines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Machine>> PostMachine(Machine machine)
        {
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMachine", new { id = machine.MachineId }, machine);
        }

        // DELETE: api/Machines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(int id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
            {
                return NotFound();
            }

            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.MachineId == id);
        }
    }
}
