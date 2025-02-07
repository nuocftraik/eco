using ECO.WebApi.Application.Notifications;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Notification;

public class NotificationController : BaseApiController
{

    [HttpPost("send-to-all")]
    [OpenApiOperation("Send notification to all users", "")]

    public Task<string> SendNotificationToAllUsers(SendNotificationRequestToAllUsersRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPost("get-notifications")]
    [OpenApiOperation("Get notifications", "")]
    public Task<PaginationResponse<NotificationDto>> GetNotifications(GetListNotificationsRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpGet("count-unread")]
    [OpenApiOperation("Count unread notifications", "")]
    public Task<int> CountUnreadNotifications()
    {
        return Mediator.Send(new CountUnreadNotificationsRequest());
    }

    [HttpPut("update-read/{id}")]
    [OpenApiOperation("Update read status", "")]
    public Task<string> UpdateReadStatus(Guid id)
    {
        return Mediator.Send(new UpdateNotificationReadRequest(id));
    }

    [HttpPut("read-all")]
    [OpenApiOperation("Read all notifications", "")]
    public Task<string> ReadAllNotifications()
    {
        return Mediator.Send(new ReadAllNotificationsRequest());
    }
}
