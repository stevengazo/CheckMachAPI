using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;

namespace CheckMachAPI.Services
{


    /// <summary>
    /// Categorías posibles para determinar dónde guardar los archivos.
    /// </summary>
    public enum FileManagerServiceTypes
    {
        Images,
        Videos,
        Documents,
        Temp,
        Logs
    }
    /// <summary>
    /// Servicio encargado de administrar la creación de carpetas y 
    /// el guardado de archivos utilizando flujos de datos (Stream).
    /// </summary>
    /// 

    public class FileManagerService
    {
        /// <summary>
        /// Provee acceso al entorno web de la aplicación, 
        /// incluyendo rutas como ContentRootPath o WebRootPath.
        /// </summary>
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Tamaño del buffer usado para operaciones de lectura/escritura.
        /// </summary>
        public const int BufferSize = 81020;

        /// <summary>
        /// Constructor del servicio FileManagerService.
        /// </summary>
        /// <param name="env">Entorno web inyectado por dependencia para obtener rutas del servidor.</param>
        public FileManagerService(IWebHostEnvironment env)
        {
            _env = env;
        }



        /// <summary>
        /// Guarda un archivo en el servidor utilizando un Stream.
        /// </summary>
        /// <param name="fileStream">Flujo de datos del archivo recibido.</param>
        /// <param name="fileName">Nombre final del archivo a guardar.</param>
        /// <param name="cancellationToken">Token para cancelar la operación asincrónica.</param>
        /// <returns>Devuelve la ruta completa donde fue guardado el archivo.</returns>
        /// <exception cref="Exception">Si el archivo ya existe en la ruta destino.</exception>
        public async Task<string> SaveFileStreamAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken, FileManagerServiceTypes typeData)
        {
            // Crear carpeta raíz "Files" si no existe
            string basePath = await CreateRootFolderAsync();

            // Crear subcarpeta "img" dentro de Files
            string imagesPath = await CreateFolderAsync(basePath, typeData.ToString());

            // Construir ruta final del archivo
            string filePath = Path.Combine(imagesPath, fileName);

            // Validación: no permitir sobrescribir archivos
            if (File.Exists(filePath))
                throw new Exception("File already exists");

            // Guardar el archivo usando FileStream
            using (var targetStream = File.Create(filePath))
            {
                // CopyToAsync copia usando el buffer proporcionado
                await fileStream.CopyToAsync(targetStream, BufferSize, cancellationToken);
            }

            return filePath;
        }

        /// <summary>
        /// Crea una carpeta dentro de una ruta base.
        /// </summary>
        /// <param name="basepath">Ruta base donde se creará la carpeta.</param>
        /// <param name="folderName">Nombre de la carpeta a crear.</param>
        /// <returns>Devuelve la ruta completa de la carpeta creada.</returns>
        private async Task<string> CreateFolderAsync(string basepath, string folderName)
        {
            string fullPath = Path.Combine(basepath, folderName);

            // Crear la carpeta solo si la ruta es válida
            if (!string.IsNullOrEmpty(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        /// <summary>
        /// Crea la carpeta raíz "Files" en el ContentRootPath si no existe.
        /// </summary>
        /// <returns>Devuelve la ruta completa de la carpeta raíz.</returns>
        private async Task<string> CreateRootFolderAsync()
        {
            // Ruta donde se almacenarán todos los archivos subidos
            string basePath = Path.Combine(_env.ContentRootPath, "Files");

            // Crear carpeta si no existe
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                return basePath;
            }

            return basePath;
        }
    }

}

