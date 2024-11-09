
using ECO.WebApi.Application.GoogleDrive.Models;
using Microsoft.AspNetCore.Http;

namespace ECO.WebApi.Application.GoogleDrive;
public interface IGoogleDriveService : ITransientService
{
    Task<List<GoogleDriveFile>> GetDriveFiles();
    Task<List<GoogleDriveFile>> GetDriveFolders();
    Task<string> UploadFileInRoot(IFormFile file);
    Task<(Stream, string, string)> DownloadFile(string fileId);
    Task DeleteFile(string fileId);
    Task<string> CreateFolderInRoot(string folderName);

    Task<string> CreateFolderInFolder(string folderName, string parentFolderId);

    Task<string> UploadFileInFolder(string folderId, IFormFile file);
    Task<List<GoogleDriveFile>> GetContainsInFolderAsync(string folderId);

    Task<bool> CheckIfFolderExitsInRoot(string folderName);
    Task<string> MoveFile(string fileId, string folderId);
    Task<string> CopyFile(string fileId, string folderId);
    Task RenameFile(string fileId, string newTitle);

    Task<bool> CheckIfFileExistsInFolder(string folderId, string fileName);
    Task CreateFolderWithPermissionAsync(string folderName, string emailAddress, string role);

    Task<string> FileSharePermission(string fileId, string emailAddress, string role);
    Task<IList<Google.Apis.Drive.v3.Data.Permission>> GetPermissionDetails(string fileId);
}

