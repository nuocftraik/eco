

using static ECO.WebApi.Shared.Notifications.BasicNotification;

namespace ECO.WebApi.Domain.Notifications;
public class Notification : AuditableEntity, IAggregateRoot
{
    public string ReceiverId { get; set; }
    public string Title { get; set; }
    public LabelType Label { get; set; }
    public string Message { get; set; }
    public string? Url { get; set; }
    public bool IsRead { get; set; }

    public Notification(string receiverId, string title, LabelType label, string message, string? url)
    {
        ReceiverId = receiverId;
        Title = title;
        Label = label;
        Message = message;
        IsRead = false;
        Url = url;
    }

    public void UpdateIsRead()
    {
        IsRead = !IsRead;
    }

}

