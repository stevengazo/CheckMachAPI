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

    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            return await _context.Items.ToListAsync();
        }

        // GET: api/Items/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Item>>> SearchItems(
            [FromQuery] string? name,
            [FromQuery] string? description,
            [FromQuery] string? author,
            [FromQuery] bool? deleted,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.Items.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(i => i.Name.ToLower().Contains(name.ToLower()));

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(i => i.Description.ToLower().Contains(description.ToLower()));

            if (!string.IsNullOrWhiteSpace(author))
                query = query.Where(i => i.Author.ToLower().Contains(author.ToLower()));

            if (deleted.HasValue)
                query = query.Where(i => i.Deleted == deleted.Value);

            if (fromDate.HasValue)
                query = query.Where(i => i.Created >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.Created <= toDate.Value);

            var results = await query
                .OrderByDescending(i => i.Created)
                .ToListAsync();

            if (!results.Any())
                return NotFound(new { Message = "No se encontraron ítems con los criterios especificados." });

            return Ok(results);
        }


        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, Item item)
        {
            if (id != item.ItemId)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
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

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItem", new { id = item.ItemId }, item);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }
    }
}
