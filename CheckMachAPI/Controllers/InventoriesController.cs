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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class InventoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventory()
        {
            return await _context.Inventory.ToListAsync();
        }

        // GET: api/Inventories/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Inventory>>> SearchInventories(
            [FromQuery] string? name,
            [FromQuery] string? location,
            [FromQuery] string? author,
            [FromQuery] bool? deleted,
            [FromQuery] DateTime? lastUpdate)
        {
            var query = _context.Inventory
                .Include(i => i.InventoryItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(i => i.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(i => i.Location.Contains(location));

            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(i => i.Author.Contains(author));

            if (deleted.HasValue)
                query = query.Where(i => i.Deleted == deleted.Value);

            if (lastUpdate.HasValue)
                query = query.Where(i => i.LastUpdate.Date == lastUpdate.Value.Date);

            var results = await query.ToListAsync();

            if (!results.Any())
                return NotFound(new { Message = "No se encontraron inventarios con los criterios especificados." });

            return Ok(results);
        }


        // GET: api/Inventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);

            if (inventory == null)
            {
                return NotFound();
            }

            return inventory;
        }

        // PUT: api/Inventories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, Inventory inventory)
        {
            if (id != inventory.InventoryId)
            {
                return BadRequest();
            }

            _context.Entry(inventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
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

        // POST: api/Inventories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory(Inventory inventory)
        {
            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventory", new { id = inventory.InventoryId }, inventory);
        }

        // DELETE: api/Inventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.Inventory.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.Inventory.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventory.Any(e => e.InventoryId == id);
        }
    }
}
