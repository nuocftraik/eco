

namespace ECO.WebApi.Application.Identity.Roles;
public class FunctionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<ActionDto> ActionDtos { get; set; }
}
