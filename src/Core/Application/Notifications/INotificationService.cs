

using ECO.WebApi.Shared.Notifications;

namespace ECO.WebApi.Application.Notifications;
public interface INotificationService : ITransientService
{
    Task SendNotificationToAllUsers(BasicNotification notification, CancellationToken cancellationToken);
    Task SendNotificationToUser(BasicNotification notification, string userId, CancellationToken cancellationToken);
    Task SendNotificationToUsers(BasicNotification notification, List<string> userIds, CancellationToken cancellationToken);
}
