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


        public async Task<string> SaveViaMultipartReaderAsync(
            string boundary,
            Stream body,
            CancellationToken cancellationToken)
        {
            var reader = new MultipartReader(boundary, body);
            MultipartSection? section = null;

            string? saveFilePath = null;

            while ((section = await reader.ReadNextSectionAsync(cancellationToken)) != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (!hasContentDispositionHeader)
                {
                    continue;
                }

                if (contentDisposition!.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName))
                {
                    string Filename = contentDisposition.FileName.Trim('"');
                    string basePath = await CreateRootFolderAsync();
                    string ImagesPath = await CreateFolderAsync(basePath, "img");
                    string FilePath = Path.Combine(ImagesPath, Filename);

                    using (var targetStream = File.Create(FilePath))
                    {
                        await section.Body.CopyToAsync(targetStream, BufferSize, cancellationToken);
                    }


                    saveFilePath = FilePath;
                }else if ( !string.IsNullOrEmpty(contentDisposition.Name) )
                {
                    using var streamReader = new StreamReader(section.Body);
                    var value = await streamReader.ReadToEndAsync(cancellationToken);

                    // Aquí puedes procesar campos normales (ej: title, description)
                    Console.WriteLine($"Campo: {contentDisposition.Name} = {value}");
                }
            }

            return saveFilePath;

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

