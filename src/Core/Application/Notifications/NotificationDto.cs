
using static ECO.WebApi.Shared.Notifications.BasicNotification;

namespace ECO.WebApi.Application.Notifications;
public class NotificationDto 
{
    public Guid Id { get; set; }
    public string ReceiverId { get; set; }
    public string Title { get; set; }
    public LabelType Label { get; set; }
    public string Message { get; set; }
    public string? Url { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedOn { get; set; }
}
