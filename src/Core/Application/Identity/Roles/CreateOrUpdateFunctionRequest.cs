

namespace ECO.WebApi.Application.Identity.Roles;
public class CreateOrUpdateFunctionRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Guid>? ActionIds { get; set; }
}
