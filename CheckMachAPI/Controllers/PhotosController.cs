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
        private readonly IWebHostEnvironment _env;

        public PhotosController(ApplicationDbContext context, FileManagerService fileManager, IWebHostEnvironment env)
        {
            _context = context;
            _file = fileManager;
        _env = env;
        }

        /// <summary>
        /// Obtiene todas las fotos y transforma su ruta física en una URL pública accesible desde el navegador.
        /// </summary>
        /// <returns>Lista de fotos con ruta accesible públicamente.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Photo>>> GetPhotos()
        {
            var photos = await _context.Photos.ToListAsync();

            // Construir URL base (http://localhost:5000 o el dominio real)
            string baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Convertir rutas físicas → URLs públicas
            foreach (var photo in photos)
            {
                if (!string.IsNullOrWhiteSpace(photo.FilePath))
                {
                    photo.FilePath = ConvertPhysicalToPublicUrl(photo.FilePath, baseUrl);
                }
            }

            return photos;
        }
        /// <summary>
        /// Convierte una ruta física del servidor en una URL accesible públicamente.
        /// </summary>
        /// <param name="physicalPath">Ruta absoluta del archivo en el servidor.</param>
        /// <param name="baseUrl">URL base del servidor (e.g. http://localhost:5000).</param>
        /// <returns>URL pública del archivo.</returns>
        private string ConvertPhysicalToPublicUrl(string physicalPath, string baseUrl)
        {
            // Obtener solo la parte después de la carpeta "Files"
            var relativePath = physicalPath
                .Replace(_env.ContentRootPath, "")
                .Replace("\\", "/");

            // Agregar el prefijo /files
            if (!relativePath.StartsWith("/Files"))
                relativePath = "/Files" + relativePath;


            return $"{baseUrl}{relativePath}";
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
        /// <summary>
        /// Endpoint para recibir archivos y campos de formulario mediante multipart/form-data,
        /// guardar los archivos usando FileManagerService y crear un registro Photo en la base de datos.
        /// </summary>
        /// <returns>Retorna información del registro creado y los archivos almacenados.</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadAndCreatePhoto()
        {
            try
            {
                /// -----------------------------------------------------------
                /// 1️⃣ VALIDAR QUE LA PETICIÓN SEA multipart/form-data
                /// -----------------------------------------------------------
                if (!Request.ContentType?.StartsWith("multipart/form-data") ?? true)
                    return BadRequest("Request must be multipart/form-data");

                // Extraer el boundary del Content-Type
                var mediaType = MediaTypeHeaderValue.Parse(Request.ContentType);
                var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

                /// -----------------------------------------------------------
                /// 2️⃣ INICIALIZAR LECTOR DE MULTIPART
                /// MultipartReader permite procesar cada sección (field o file)
                /// sin cargar todo el request en memoria.
                /// -----------------------------------------------------------
                var reader = new MultipartReader(boundary, Request.Body);

                // Diccionario para campos de formulario
                var formFields = new Dictionary<string, string>();

                // Lista de archivos guardados
                var savedFiles = new List<string>();

                MultipartSection? section;

                /// -----------------------------------------------------------
                /// 3️⃣ LEER SECCIÓN POR SECCIÓN DEL MULTIPART
                /// Cada "section" es un archivo o un campo normal.
                /// -----------------------------------------------------------
                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    var hasContentDispositionHeader =
                        ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                    if (!hasContentDispositionHeader)
                        continue;

                    /// -----------------------------------------------------------
                    /// 4️⃣ PROCESAR ARCHIVO (input type=file)
                    /// - Si tiene FileName, entonces es un archivo.
                    /// - Se envía directo al FileManagerService.
                    /// -----------------------------------------------------------
                    if (contentDisposition!.DispositionType == "form-data" &&
                        !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        string storedPath = await _file.SaveFileStreamAsync(
                                                                                section.Body,
                                                                                contentDisposition.FileName.Value!,
                                                                                HttpContext.RequestAborted,
                                                                                FileManagerServiceTypes.Images
                                                                            );

                        savedFiles.Add(storedPath);
                        continue;
                    }

                    /// -----------------------------------------------------------
                    /// 5️⃣ PROCESAR CAMPOS DE FORMULARIO (input type=text / hidden)
                    /// -----------------------------------------------------------
                    if (contentDisposition.DispositionType == "form-data")
                    {
                        using var readerField = new StreamReader(section.Body);
                        string fieldValue = await readerField.ReadToEndAsync();

                        // Registrar el valor del campo usando su nombre
                        formFields[contentDisposition.Name.Value!] = fieldValue;
                    }
                }

                /// -----------------------------------------------------------
                /// 6️⃣ VALIDAR CAMPOS REQUERIDOS DEL MODELO Photo
                /// -----------------------------------------------------------
                if (!formFields.ContainsKey("ReferenceId") ||
                    !formFields.ContainsKey("PhotoType"))
                {
                    return BadRequest("ReferenceId y PhotoType son obligatorios");
                }

                /// -----------------------------------------------------------
                /// 7️⃣ CREAR INSTANCIA DEL MODELO Photo CON LOS DATOS RECIBIDOS
                /// -----------------------------------------------------------
                var photo = new Photo
                {
                    FilePath = savedFiles.FirstOrDefault(),
                    Description = formFields.GetValueOrDefault("Description"),
                    ReferenceId = int.Parse(formFields["ReferenceId"]),
                    PhotoType = formFields["PhotoType"]
                };

                /// -----------------------------------------------------------
                /// 8️⃣ GUARDAR EN BASE DE DATOS
                /// -----------------------------------------------------------
                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                /// -----------------------------------------------------------
                /// 9️⃣ RETORNAR RESPUESTA EXITOSA
                /// -----------------------------------------------------------
                return Ok(new
                {
                    Message = "Photo created successfully",
                    Photo = photo,
                    Files = savedFiles
                });
            }
            catch (Exception ex)
            {
                /// -----------------------------------------------------------
                /// 🔟 ERROR → RETORNAR BAD REQUEST CON MENSAJE
                /// -----------------------------------------------------------
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
