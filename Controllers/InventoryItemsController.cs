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
    public class InventoryItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventoryItems()
        {
            return await _context.InventoryItems.ToListAsync();
        }

        // GET: api/InventoryItems/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> SearchInventoryItems(
            [FromQuery] int? itemId,
            [FromQuery] int? inventoryId,
            [FromQuery] string? itemName,
            [FromQuery] int? minStockBelow,
            [FromQuery] DateTime? createdAfter,
            [FromQuery] DateTime? createdBefore)
        {
            var query = _context.InventoryItems
                .Include(ii => ii.Item)
                .Include(ii => ii.Inventory)
                .Include(ii => ii.InventoryMoves)
                .AsQueryable();

            if (itemId.HasValue)
                query = query.Where(ii => ii.ItemId == itemId.Value);

            if (inventoryId.HasValue)
                query = query.Where(ii => ii.InventoryId == inventoryId.Value);

            if (!string.IsNullOrWhiteSpace(itemName))
                query = query.Where(ii => ii.Item.Name.Contains(itemName));

            if (minStockBelow.HasValue)
                query = query.Where(ii => ii.Quantity < minStockBelow.Value);

            if (createdAfter.HasValue)
                query = query.Where(ii => ii.Created >= createdAfter.Value);

            if (createdBefore.HasValue)
                query = query.Where(ii => ii.Created <= createdBefore.Value);

            var results = await query.ToListAsync();

            if (!results.Any())
                return NotFound(new { Message = "No se encontraron items de inventario con los criterios especificados." });

            return Ok(results);
        }


        // GET: api/InventoryItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItem(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);

            if (inventoryItem == null)
            {
                return NotFound();
            }

            return inventoryItem;
        }

        // PUT: api/InventoryItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventoryItem(int id, InventoryItem inventoryItem)
        {
            if (id != inventoryItem.InventoryItemId)
            {
                return BadRequest();
            }

            _context.Entry(inventoryItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(id))
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

        // POST: api/InventoryItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> PostInventoryItem(InventoryItem inventoryItem)
        {
            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventoryItem", new { id = inventoryItem.InventoryItemId }, inventoryItem);
        }

        // DELETE: api/InventoryItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItem(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }

            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.InventoryItemId == id);
        }
    }
}
