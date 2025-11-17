using CheckMachAPI.Data;
using CheckMachAPI.DTO;
using CheckMachAPI.Models;
using CheckMachAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckMachAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequestSizeLimit(long.MaxValue)]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]

    public class PhotosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly FileManagerService _file;

        public PhotosController(ApplicationDbContext context, FileManagerService fileManager)
        {
            _context = context;
            _file = fileManager;
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadAndCreatePhoto()
        {
            try
            {
                if (!Request.ContentType?.StartsWith("multipart/form-data") ?? true)
                    return BadRequest("Request must be multipart/form-data");

                var mediaType = MediaTypeHeaderValue.Parse(Request.ContentType);
                var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

                // 1️⃣ Leer partes del multipart
                var reader = new MultipartReader(boundary, Request.Body);

                var formFields = new Dictionary<string, string>();
                var savedFiles = new List<string>();

                MultipartSection? section;
                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    var hasContentDispositionHeader =
                        ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                  
                    if (!hasContentDispositionHeader)
                        continue;

                    // 2️⃣ SI ES ARCHIVO → lo envías a tu FileManager
                    if (contentDisposition!.DispositionType == "form-data" &&
                        !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        string storedPath = await _file.SaveFileStreamAsync(
                                                                               section.Body,
                                                                               contentDisposition.FileName.Value!,
                                                                               HttpContext.RequestAborted
                                                                           );
                        savedFiles.Add(storedPath);
                        continue;
                    }

                    // 3️⃣ SI ES FORM FIELD → lo agregas al diccionario
                    if (contentDisposition.DispositionType == "form-data")
                    {
                        using var readerField = new StreamReader(section.Body);
                        string fieldValue = await readerField.ReadToEndAsync();
                        formFields[contentDisposition.Name.Value!] = fieldValue;
                    }
                }

                // 4️⃣ Validar campos requeridos del modelo
                if (!formFields.ContainsKey("ReferenceId") ||
                    !formFields.ContainsKey("PhotoType"))
                {
                    return BadRequest("ReferenceId y PhotoType son obligatorios");
                }

                // 5️⃣ Crear modelo Photo
                var photo = new Photo
                {
                    FilePath = savedFiles.FirstOrDefault(),
                    Description = formFields.GetValueOrDefault("Description"),
                    ReferenceId = int.Parse(formFields["ReferenceId"]),
                    PhotoType = formFields["PhotoType"]
                };

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Photo created successfully",
                    Photo = photo,
                    Files = savedFiles
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
