using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;

namespace CheckMachAPI.Services
{
    public class FileManagerService
    {
        /// <summary>
        /// Get the web enviroment
        /// </summary>
        private readonly IWebHostEnvironment _env;
        /// <summary>
        /// BufferSize 
        /// </summary>
        public const int BufferSize = 81020;


        /// <summary>
        /// Constructor of the class
        /// </summary>
        /// <param name="env"></param>
        public FileManagerService(IWebHostEnvironment env)
        {
            _env = env;
        }


        public async Task<string> SaveFileStreamAsync(
          Stream fileStream,
          string fileName,
          CancellationToken cancellationToken)
        {
            // Crear carpeta raíz si no existe
            string basePath = await CreateRootFolderAsync();

            // Crear subcarpeta img
            string imagesPath = await CreateFolderAsync(basePath, "img");

            // Ruta final del archivo
            string filePath = Path.Combine(imagesPath, fileName);

            // Validación
            if (File.Exists(filePath))
                throw new Exception("File already exists");

            // Guardar archivo
            using (var targetStream = File.Create(filePath))
            {
                await fileStream.CopyToAsync(targetStream, 81920, cancellationToken);
            }

            return filePath;
        }



        private async Task<string> CreateFolderAsync(string basepath,string folderName)
        {
            string fullPath = Path.Combine(basepath, folderName);
            if (!string.IsNullOrEmpty(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        private async Task<string> CreateRootFolderAsync()
        {
            string basePath = Path.Combine(_env.ContentRootPath, "Files");
            if(!Directory.Exists(basePath))
            {
                Directory.CreateDirectory($"{basePath}");
                return basePath;
            }
            return basePath;
        }
    }
}

