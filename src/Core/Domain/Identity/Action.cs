

namespace ECO.WebApi.Domain.Identity;
public class Action : BaseEntity
{
    public string Name { get; set; }
    public virtual ICollection<ActionInFunction> ActionInFunctions { get; set; }

    public Action()
    {
        
    }
    public Action(string name)
    {
        Name = name;
    }
}
