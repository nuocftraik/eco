using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Common;

namespace ECO.WebApi.Application.Common.FileStorage;
public interface IFileStorageService : ITransientService
{
    public Task<string> UploadAsync<T>(FileUploadRequest? request, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class;

    public void Remove(string? path);
}
