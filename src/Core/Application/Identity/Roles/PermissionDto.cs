

namespace ECO.WebApi.Application.Identity.Roles;
public class PermissionDto
{
    public required string Type { get; set; }
    public required string Value { get; set; }
    public string? DisplayName { get; set; }
    public bool Selected { get; set; }
}
