

namespace ECO.WebApi.Application.Common.BlobStorage;
public class BlobModel
{
    public string ContainerName { get; set; }
    public string BlobName { get; set; }
    public string BlobURL { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedDate { get; set; }
}
