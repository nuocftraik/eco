

namespace ECO.WebApi.Application.GoogleDrive.Models;
public class GoogleDriveFile
{
    public string Id { get; set; }
    public string Name { get; set; }

    public long? Size { get; set; }
    public long? Version { get; set; }
    public string MimeType { get; set; }
    public IList<string> Parents { get; set; }
    public DateTime? CreatedTime { get; set; }

}
