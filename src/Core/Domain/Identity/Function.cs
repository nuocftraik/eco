

using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Domain.Identity;
public class Function : BaseEntity
{
    public string Name { get; set; }
    public virtual List<ActionInFunction> ActionInFunctions { get; set; } = new();

    public Function()
    {
        
    }

    //write function add action to function
    public void AddAction(Guid actionId)
    {
        ActionInFunctions.Add(new ActionInFunction(actionId,Id));
    }

    //write function update actions in function
    public void UpdateActions(List<Guid>? newActionIds)
    {
        if (newActionIds == null || newActionIds.Count == 0)
        {
            ActionInFunctions.Clear();
            return;
        }
        // Xóa các action không có trong danh sách mới
        ActionInFunctions.RemoveAll(pc => !newActionIds.Contains(pc.ActionId));

        // Thêm các action mới chưa có trong danh mục
        var existingActionIds = ActionInFunctions.Select(pc => pc.ActionId).ToHashSet(); // Chuyển sang HashSet để tối ưu hóa Contains
        foreach (var actionId in newActionIds)
        {
            if (!existingActionIds.Contains(actionId))
            {
                ActionInFunctions.Add(new ActionInFunction(actionId, Id));
            }
        }
    }


}
