
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Domain.Identity;
[PrimaryKey(nameof(ActionId), nameof(FunctionId))]

public class ActionInFunction
{
    public Guid ActionId { get; set; }
    public Guid FunctionId { get; set; }
    public virtual Action Action { get; set; }
    public virtual Function Function { get; set; }
    public ActionInFunction()
    {
        
    }
    public ActionInFunction(Guid actionId, Guid functionId)
    {
        ActionId = actionId;
        FunctionId = functionId;
    }
}
