

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Domain.Identity;
[PrimaryKey(nameof(RoleId), nameof(FunctionId),nameof(ActionId))]

public class Permission
{
    public string RoleId { get; set; }
    public Guid FunctionId { get; set; }
    public Guid ActionId { get; set; }
    [ForeignKey("RoleId")]
    public virtual ApplicationRole Role { get; set; }
    public virtual Function Function { get; set; }
    public virtual Action Action { get; set; }

    public Permission()
    {
        
    }
    public Permission(string roleId, Guid functionId, Guid actionId)
    {
        RoleId = roleId;
        FunctionId = functionId;
        ActionId = actionId;
    }
}
