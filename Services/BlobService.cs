using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CheckMachAPI.Settings;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(BlobSettings settings)
    {
        var blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
        return blobClient.Uri.ToString(); // Devuelve la URL pública del archivo
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.DeleteIfExistsAsync();
    }
}
