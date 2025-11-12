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
    public class InventoryMovesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryMovesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryMoves
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryMove>>> GetInventoryMoves()
        {
            return await _context.InventoryMoves.ToListAsync();
        }

        // GET: api/InventoryMoves/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<InventoryMove>>> SearchInventoryMoves(
            [FromQuery] string? movementType,
            [FromQuery] int? inventoryItemId,
            [FromQuery] string? userId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int? minQuantity,
            [FromQuery] int? maxQuantity,
            [FromQuery] string? notes)
        {
            var query = _context.InventoryMoves
                .Include(m => m.User)
                .Include(m => m.InventoryItem)
                .ThenInclude(i => i.Item)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(movementType))
                query = query.Where(m => m.MovementType.ToLower().Contains(movementType.ToLower()));

            if (inventoryItemId.HasValue)
                query = query.Where(m => m.InventoryItemId == inventoryItemId.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(m => m.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(m => m.MovementDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.MovementDate <= toDate.Value);

            if (minQuantity.HasValue)
                query = query.Where(m => m.Quantity >= minQuantity.Value);

            if (maxQuantity.HasValue)
                query = query.Where(m => m.Quantity <= maxQuantity.Value);

            if (!string.IsNullOrWhiteSpace(notes))
                query = query.Where(m => m.Notes.Contains(notes));

            var results = await query
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            if (!results.Any())
                return NotFound(new { Message = "No se encontraron movimientos con los criterios especificados." });

            return Ok(results);
        }


        // GET: api/InventoryMoves/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryMove>> GetInventoryMove(int id)
        {
            var inventoryMove = await _context.InventoryMoves.FindAsync(id);

            if (inventoryMove == null)
            {
                return NotFound();
            }

            return inventoryMove;
        }

        // PUT: api/InventoryMoves/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventoryMove(int id, InventoryMove inventoryMove)
        {
            if (id != inventoryMove.InventoryMoveId)
            {
                return BadRequest();
            }

            _context.Entry(inventoryMove).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryMoveExists(id))
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

        // POST: api/InventoryMoves
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InventoryMove>> PostInventoryMove(InventoryMove inventoryMove)
        {
            _context.InventoryMoves.Add(inventoryMove);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInventoryMove", new { id = inventoryMove.InventoryMoveId }, inventoryMove);
        }

        // DELETE: api/InventoryMoves/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryMove(int id)
        {
            var inventoryMove = await _context.InventoryMoves.FindAsync(id);
            if (inventoryMove == null)
            {
                return NotFound();
            }

            _context.InventoryMoves.Remove(inventoryMove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryMoveExists(int id)
        {
            return _context.InventoryMoves.Any(e => e.InventoryMoveId == id);
        }
    }
}
