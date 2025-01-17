

namespace ECO.WebApi.Domain.Identity;
public class Action : BaseEntity<string>
{
    public string Name { get; set; }
    public virtual ICollection<Permission> Permissions { get; set; }
    public virtual ICollection<ActionInFunction> ActionInFunctions { get; set; }
}
