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

    public class PhotosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PhotosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Photos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        {
            return await _context.Photos.ToListAsync();
        }

        // GET: api/Machines/searchFull?name=Excavator&type=Preventive&photoType=Machine
        [HttpGet("searchFull")]
        public async Task<ActionResult<IEnumerable<Machine>>> SearchMachinesFull(
            [FromQuery] string? name,
            [FromQuery] string? model,
            [FromQuery] string? location,
            [FromQuery] string? maintenanceType,
            [FromQuery] string? maintenanceStatus,
            [FromQuery] string? maintenanceAuthor,
            [FromQuery] string? photoType)
        {
            var query = _context.Machines.AsQueryable();

            // Filtro por propiedades de la máquina
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(m => m.Name != null && m.Name.Contains(name));
            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(m => m.Model != null && m.Model.Contains(model));
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(m => m.Location != null && m.Location.Contains(location));

            // Filtro por mantenimientos
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

            // Incluimos mantenimientos relacionados
            query = query.Include(m => m.Maintenances);

            // Filtrado de fotos
            if (!string.IsNullOrWhiteSpace(photoType))
            {
                query = query.Where(m => _context.Photos.Any(p => p.ReferenceId == m.MachineId && p.PhotoType != null && p.PhotoType.Contains(photoType)));
            }

            var result = await query.ToListAsync();

            if (result.Count == 0)
                return NotFound("No se encontraron máquinas que cumplan los criterios especificados.");

            return Ok(result);
        }


        // GET: api/Photos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Photo>> GetPhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);

            if (photo == null)
            {
                return NotFound();
            }

            return photo;
        }

        // PUT: api/Photos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoto(int id, Photo photo)
        {
            if (id != photo.PhotoId)
            {
                return BadRequest();
            }

            _context.Entry(photo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhotoExists(id))
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

        // POST: api/Photos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Photo>> PostPhoto(Photo photo)
        {
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhoto", new { id = photo.PhotoId }, photo);
        }

        // DELETE: api/Photos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PhotoExists(int id)
        {
            return _context.Photos.Any(e => e.PhotoId == id);
        }
    }
}
