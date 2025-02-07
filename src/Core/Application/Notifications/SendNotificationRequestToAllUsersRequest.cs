using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECO.WebApi.Shared.Notifications;

namespace ECO.WebApi.Application.Notifications;
public class SendNotificationRequestToAllUsersRequest : IRequest<string>
{
    public BasicNotification Notification { get; set; }
}

public class SendNotificationRequestHandler : IRequestHandler<SendNotificationRequestToAllUsersRequest, string>
{
    private readonly INotificationService _notificationService;
    public SendNotificationRequestHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<string> Handle(SendNotificationRequestToAllUsersRequest request, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationToAllUsers(request.Notification, cancellationToken);
        return "Notification sent";
    }
}
