using ECO.WebApi.Application.GoogleDrive.Models;
using ECO.WebApi.Application.GoogleDrive;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.GoogleDrive;
[AllowAnonymous]
public class GoogleDriveController : BaseApiController
{
    private readonly IGoogleDriveService _googleDriveService;

    public GoogleDriveController(IGoogleDriveService googleDriveService)
    {
        _googleDriveService = googleDriveService;
    }

    [HttpGet("files")]
    [OpenApiOperation("Get all files from Google Drive.", "")]
    public async Task<List<GoogleDriveFile>> GetDriveFiles()
    {
        return await _googleDriveService.GetDriveFiles();
    }

    [HttpGet("folders")]
    [OpenApiOperation("Get all folders from Google Drive.", "")]
    public async Task<List<GoogleDriveFile>> GetDriveFolders()
    {
        return await _googleDriveService.GetDriveFolders();
    }

    [HttpPost("upload-file-root")]
    [OpenApiOperation("Upload a file to the root directory in Google Drive.", "")]
    public async Task<string> UploadFileInRoot(IFormFile file)
    {
        return await _googleDriveService.UploadFileInRoot(file);
    }

    [HttpGet("download-file/{fileId}")]
    [OpenApiOperation("Download a file from Google Drive by ID.", "")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        var stream = await _googleDriveService.DownloadFile(fileId);
        if (stream == null)
            return NotFound("File not found.");

        return File(stream, "application/octet-stream");
    }

    [HttpDelete("delete-file/{fileId}")]
    [OpenApiOperation("Delete a file from Google Drive by ID.", "")]
    public async Task DeleteFile(string fileId)
    {
        await _googleDriveService.DeleteFile(fileId);
    }

    [HttpPost("create-folder-root")]
    [OpenApiOperation("Create a folder in the root directory of Google Drive.", "")]
    public async Task<string> CreateFolderInRoot(string folderName)
    {
        return await _googleDriveService.CreateFolderInRoot(folderName);
    }

    [HttpPost("create-folder-in-folder")]
    [OpenApiOperation("Create a folder inside another folder on Google Drive.", "")]
    public async Task<string> CreateFolderInFolder(string folderName, string parentFolderId)
    {
        return await _googleDriveService.CreateFolderInFolder(folderName, parentFolderId);
    }

    [HttpPost("upload-file-in-folder")]
    [OpenApiOperation("Upload a file into a specified folder on Google Drive.", "")]
    public async Task<string> UploadFileInFolder(string folderId, IFormFile file)
    {
        return await _googleDriveService.UploadFileInFolder(folderId, file);
    }

    [HttpGet("get-folder-contents/{folderId}")]
    [OpenApiOperation("Get contents of a specified folder from Google Drive.", "")]
    public async Task<List<GoogleDriveFile>> GetContainsInFolderAsync(string folderId)
    {
        return await _googleDriveService.GetContainsInFolderAsync(folderId);
    }

    [HttpGet("check-folder-exists-root")]
    [OpenApiOperation("Check if a folder exists in the root of Google Drive by name.", "")]
    public async Task<bool> CheckIfFolderExistsInRoot(string folderName)
    {
        return await _googleDriveService.CheckIfFolderExitsInRoot(folderName);
    }

    [HttpPost("move-file")]
    [OpenApiOperation("Move a file to another folder on Google Drive.", "")]
    public async Task<string> MoveFile(string fileId, string folderId)
    {
        return await _googleDriveService.MoveFile(fileId, folderId);
    }

    [HttpPost("copy-file")]
    [OpenApiOperation("Copy a file to another folder on Google Drive.", "")]
    public async Task<string> CopyFile(string fileId, string folderId)
    {
        return await _googleDriveService.CopyFile(fileId, folderId);
    }

    [HttpPut("rename-file")]
    [OpenApiOperation("Rename a file on Google Drive by ID.", "")]
    public async Task RenameFile(string fileId, string newTitle)
    {
        await _googleDriveService.RenameFile(fileId, newTitle);
    }

    [HttpGet("check-file-exists")]
    [OpenApiOperation("Check if a file exists in a specific folder on Google Drive by name.", "")]
    public async Task<bool> CheckIfFileExistsInFolder(string folderId, string fileName)
    {
        return await _googleDriveService.CheckIfFileExistsInFolder(folderId, fileName);
    }

    [HttpPost("create-folder-with-permission")]
    [OpenApiOperation("Create a folder with specific sharing permissions on Google Drive.", "")]
    public async Task CreateFolderWithPermissionAsync(string folderName, string emailAddress, string role)
    {
        await _googleDriveService.CreateFolderWithPermissionAsync(folderName, emailAddress, role);
    }

    [HttpPost("share-file-permission")]
    [OpenApiOperation("Share a file with specific permissions on Google Drive.", "")]
    public async Task<string> FileSharePermission(string fileId, string emailAddress, string role)
    {
        return await _googleDriveService.FileSharePermission(fileId, emailAddress, role);
    }

    [HttpGet("get-permission-details/{fileId}")]
    [OpenApiOperation("Get permission details of a file on Google Drive.", "")]
    public async Task<IList<Google.Apis.Drive.v3.Data.Permission>> GetPermissionDetails(string fileId)
    {
        return await _googleDriveService.GetPermissionDetails(fileId);
    }
}
