

namespace ECO.WebApi.Application.Common.BlobStorage;
public interface IBlobStorageService
{
    // Container methods
    Task<List<BlobContainerModel>> GetListBlobContainersAsync();
    Task<bool> DeleteContainerIfExistAsync(string containerName);
    Task<bool> CreateContainerAsync(string containerName);

    // Blob methods
    Task<List<BlobModel>> GetListBlobAsync(string containerName);
    Task<bool> DeleteBlobIfExistAsync(string containerName, string fileName);
    Task UploadBlobAsync(Stream mediaBinaryStream, string blobName, string containerName, string blobContentTpye);
}
