
using ECO.WebApi.Domain.Notifications;

namespace ECO.WebApi.Application.Notifications;
public class ReadAllNotificationsRequest : IRequest<string>
{
}

public class ReadAllNotificationsRequestSpec : Specification<Notification>
{
    public ReadAllNotificationsRequestSpec(string userId)
    {
        Query.Where(x => !x.IsRead && x.ReceiverId.Equals(userId));
    }
}

public class ReadAllNotificationsRequestHandler : IRequestHandler<ReadAllNotificationsRequest, string>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public ReadAllNotificationsRequestHandler(IRepository<Notification> notificationRepository, ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(ReadAllNotificationsRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        var spec = new ReadAllNotificationsRequestSpec(userId);
        var notifications = await _notificationRepository.ListAsync(spec, cancellationToken);

        foreach (var notification in notifications)
        {
            notification.UpdateIsRead();
            await _notificationRepository.UpdateAsync(notification, cancellationToken);
        }

        return "All notifications read status updated successfully.";
    }
}

