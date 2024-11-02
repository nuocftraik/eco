
using ECO.WebApi.Application.GoogleDrive.Models;
using ECO.WebApi.Application.GoogleDrive;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Http;
using Google.Apis.Download;

namespace ECO.WebApi.Infrastructure.GoogleDrive;
public class GoogleDriveService : IGoogleDriveService
{
    private readonly DriveService _driveService;

    public GoogleDriveService(DriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task<List<GoogleDriveFile>> GetDriveFiles()
    {
        var request = _driveService.Files.List();
        request.Fields = "nextPageToken, files(*)";
        var files = (await request.ExecuteAsync()).Files;
        List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
        if (files != null && files.Count > 0)
        {
            foreach (var file in files)
            {
                GoogleDriveFile File = new GoogleDriveFile
                {
                    Id = file.Id,
                    Name = file.Name,
                    Size = file.Size,
                    Version = file.Version,
                    CreatedTime = file.CreatedTime,
                    Parents = file.Parents,
                    MimeType = file.MimeType
                };
                FileList.Add(File);
            }
        }
        return FileList;
    }

    public async Task<List<GoogleDriveFile>> GetDriveFolders()
    {
        List<GoogleDriveFile> FolderList = new List<GoogleDriveFile>();

        Google.Apis.Drive.v3.FilesResource.ListRequest request = _driveService.Files.List();
        request.Q = "mimeType='application/vnd.google-apps.folder'";
        request.Fields = "files(id, name, size, version, createdTime)";

        Google.Apis.Drive.v3.Data.FileList result = request.Execute();
        foreach (var file in result.Files)
        {
            GoogleDriveFile File = new GoogleDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                Version = file.Version,
                CreatedTime = file.CreatedTime
            };
            FolderList.Add(File);
        }
        return FolderList;
    }

    public async Task<string> UploadFileInRoot(IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new ArgumentException("File is not valid.");
        }

        try
        {
            // Create metadata for the file to upload to the root of Google Drive
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.FileName
            };

            // Upload the file directly to Google Drive root without saving it locally
            using (var fileStream = file.OpenReadStream())
            {
                var request = _driveService.Files.Create(fileMetadata, fileStream, file.ContentType);
                request.Fields = "id"; // Specify that we want to return the file ID
                await request.UploadAsync();

                // Return the ID of the uploaded file
                return request.ResponseBody?.Id;
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw new Exception("Error uploading file to Google Drive.", ex);
        }
    }

    public async Task<Stream> DownloadFile(string fileId)
    {
        var request = _driveService.Files.Get(fileId);
        var memoryStream = new MemoryStream();

        // Tải tệp xuống và sao chép vào memoryStream
        await request.DownloadAsync(memoryStream);

        // Đặt vị trí về đầu stream để có thể đọc lại từ đầu
        memoryStream.Position = 0;
        return memoryStream;

    }

    public async Task<string> DownloadGoogleFileAsync(string fileId, string destinationFolderPath)
    {
        var request = _driveService.Files.Get(fileId);

        // Lấy thông tin tên tệp từ Google Drive
        var fileMetadata = await request.ExecuteAsync();
        string fileName = fileMetadata.Name;
        string filePath = Path.Combine(destinationFolderPath, fileName);

        // Tạo MemoryStream để lưu tạm dữ liệu tải về
        using var memoryStream = new MemoryStream();

        // Thêm handler để xử lý tiến trình tải xuống
        request.MediaDownloader.ProgressChanged += progress =>
        {
            switch (progress.Status)
            {
                case DownloadStatus.Downloading:
                    break;
                case DownloadStatus.Completed:
                    SaveStream(memoryStream, filePath);
                    break;
                case DownloadStatus.Failed:
                    break;
            }
        };

        await request.DownloadAsync(memoryStream);
        return filePath;
    }

    // Hàm lưu stream vào tệp trên hệ thống
    private void SaveStream(Stream input, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        input.Seek(0, SeekOrigin.Begin);
        input.CopyTo(fileStream);
    }


    public async Task DeleteFile(string fileId)
    {
        await _driveService.Files.Delete(fileId).ExecuteAsync();
    }

    public async Task<string> CreateFolderInRoot(string folderName)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder"
        };
        var folder = await _driveService.Files.Create(fileMetadata).ExecuteAsync();
        return folder.Id;
    }
    public async Task<string> CreateFolderInFolder(string folderName, string parentFolderId)
    {
        // Thiết lập thông tin metadata cho thư mục mới
        var fileMetaData = new Google.Apis.Drive.v3.Data.File()
        {
            Name = folderName, // Tên thư mục sẽ được tạo
            MimeType = "application/vnd.google-apps.folder", // Đặt mimeType để xác định đây là thư mục
            Parents = new List<string> { parentFolderId } // Đặt thư mục cha
        };

        // Gửi yêu cầu tạo thư mục
        var request = _driveService.Files.Create(fileMetaData);
        request.Fields = "id"; // Chỉ yêu cầu trường 'id' trong kết quả trả về để tiết kiệm băng thông

        // Thực hiện yêu cầu và chờ kết quả
        var folder = await request.ExecuteAsync();

        // Trả về Id của thư mục mới tạo
        return folder.Id;
    }
    public async Task<string> UploadFileInFolder(string folderId, IFormFile file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new ArgumentException("File is not valid.");
        }

        try
        {
            // Create metadata for the file to upload to Google Drive
            var fileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.FileName,
                MimeType = file.ContentType,
                Parents = new List<string> { folderId }
            };

            // Upload the file to Google Drive without saving it locally
            using (var stream = file.OpenReadStream())
            {
                var request = _driveService.Files.Create(fileMetaData, stream, file.ContentType);
                request.Fields = "id";
                await request.UploadAsync();
            }

            // Return the ID of the uploaded file
            return "File uploaded successfully.";
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw new Exception("Error uploading file to Google Drive.", ex);
        }
    }


    public async Task<List<GoogleDriveFile>> GetContainsInFolderAsync(string folderId)
    {
        // Tạo danh sách lưu trữ ID của các tệp con trong thư mục
        List<string> childList = new List<string>();

        // Tạo yêu cầu để lấy danh sách các tệp con trong thư mục dựa trên folderId
        var request = _driveService.Files.List();
        request.Q = $"'{folderId}' in parents";
        request.Fields = "nextPageToken, files(id, name, mimeType, size, createdTime, parents, version)";

        // Lặp qua các trang của danh sách các tệp con trong thư mục
        do
        {
            // Thực hiện yêu cầu và lấy kết quả là các tệp con
            var result = await request.ExecuteAsync();

            // Thêm ID của từng tệp vào danh sách childList
            if (result.Files != null && result.Files.Count > 0)
            {
                foreach (var file in result.Files)
                {
                    childList.Add(file.Id);
                }
            }

            // Lấy token cho trang tiếp theo, nếu có
            request.PageToken = result.NextPageToken;

        } while (!string.IsNullOrEmpty(request.PageToken));

        // Lấy danh sách tất cả các tệp từ Google Drive (không lọc)
        var allFiles = await GetDriveFiles();

        // Lọc các tệp có ID nằm trong danh sách childList
        var filteredFiles = allFiles.Where(file => childList.Contains(file.Id)).ToList();

        return filteredFiles;
    }

    // Check if a folder exists in the root directory
    public async Task<bool> CheckIfFolderExitsInRoot(string folderName)
    {
        bool isExist = false;

        // Define the parameters of the request.
        var fileListRequest = _driveService.Files.List();
        fileListRequest.Fields = "nextPageToken, files(id, name, mimeType)";

        // List files and filter for folders
        var files = await fileListRequest.ExecuteAsync();

        // Check if any folder with the specified name exists
        isExist = files.Files.Any(x => x.MimeType == "application/vnd.google-apps.folder" && x.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

        return isExist;
    }

    public async Task<string> MoveFile(string fileId, string folderId)
    {
        try
        {
            // Retrieve the existing parents to remove
            var getRequest = _driveService.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = await getRequest.ExecuteAsync();
            string previousParents = string.Join(",", file.Parents);

            // Move the file to the new folder
            var updateRequest = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;

            var updatedFile = await updateRequest.ExecuteAsync();

            return updatedFile != null ? "Success" : "Fail";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> CopyFile(string fileId, string folderId)
    {
        try
        {
            // Create metadata for the new file copy
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Parents = new List<string> { folderId }
            };

            // Copy the file to the new folder
            var copyRequest = _driveService.Files.Copy(fileMetadata, fileId);
            var copiedFile = await copyRequest.ExecuteAsync();

            // Return the ID of the copied file for further reference
            return copiedFile != null ? $"File copied successfully. New File ID: {copiedFile.Id}" : "File copy failed.";
        }
        catch (Exception ex)
        {
            // Handle exceptions (logging, rethrowing, etc.)
            return $"Error copying file: {ex.Message}";
        }
    }



    public async Task RenameFile(string fileId, string newTitle)
    {
        var file = new Google.Apis.Drive.v3.Data.File { Name = newTitle };
        var request = _driveService.Files.Update(file, fileId);
        await request.ExecuteAsync();
    }


    // Move multiple files to a new folder
    public async Task MoveMultipleFiles(List<string> fileIds, string folderId)
    {

        foreach (var fileId in fileIds)
        {
            // Retrieve the existing parents to remove
            var getRequest = _driveService.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = getRequest.Execute();
            string previousParents = string.Join(",", file.Parents);

            // Move the file to the new folder
            var updateRequest = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;
            updateRequest.Execute();
        }
    }

    // Check if a file exists in a folder by file name
    public async Task<bool> CheckIfFileExistsInFolder(string folderId, string fileName)
    {

        var request = _driveService.Files.List();
        request.Q = $"'{folderId}' in parents and name = '{fileName}'"; // Query for file in the specified folder
        request.Fields = "files(id)";

        var files = request.Execute().Files;
        return files != null && files.Count > 0; // Return true if file exists
    }


    public async Task CreateFolderWithPermissionAsync(string folderName, string emailAddress, string role)
    {
        string message = string.Empty;
        // Tạo thư mục trong Google Drive
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
            Description = $"File created at {DateTime.Now:HH:mm:ss}"
        };
        // Tạo thư mục và lấy ID thư mục
        var createRequest = _driveService.Files.Create(fileMetadata);
        createRequest.Fields = "id";
        var folder = await createRequest.ExecuteAsync();

        // Xác định quyền truy cập
        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "user",
            EmailAddress = emailAddress,
            Role = role
        };

        var permissionsRequest = _driveService.Permissions.Create(permission, folder.Id);
        if (role == "owner")
        {
            permissionsRequest.TransferOwnership = true;
        }

        // Thiết lập quyền truy cập
        await permissionsRequest.ExecuteAsync();

    }


    public async Task<string> FileSharePermission(string fileId, string emailAddress, string userRole)
    {

        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "user",
            EmailAddress = emailAddress,
            Role = userRole // "reader" hoặc "writer"
        };

        if (userRole == "writer" || userRole == "reader")
        {
            var request = _driveService.Permissions.Create(permission, fileId);
            await request.ExecuteAsync();
            return "Success";
        }
        else
        {
            return "Invalid role provided. Only 'reader' or 'writer' are allowed in this method.";
        }

    }

    public async Task<IList<Google.Apis.Drive.v3.Data.Permission>> GetPermissionDetails(string fileId)
    {
        var getRequest = _driveService.Files.Get(fileId);
        getRequest.Fields = "permissions";

        var file = await getRequest.ExecuteAsync();
        return file.Permissions;

    }

}

//"reader": Người dùng có thể xem thư mục và nội dung bên trong nhưng không thể chỉnh sửa.
//"writer": Người dùng có thể chỉnh sửa, thêm và xóa nội dung trong thư mục.
//"owner": Người dùng được quyền sở hữu thư mục và có toàn quyền kiểm soát, bao gồm việc chia sẻ lại và thay đổi quyền.

