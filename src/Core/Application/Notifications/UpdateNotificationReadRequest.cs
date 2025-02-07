using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECO.WebApi.Domain.Notifications;

namespace ECO.WebApi.Application.Notifications;
public class UpdateNotificationReadRequest : IRequest<string>
{
    public Guid NotificationId { get; set; }

    public UpdateNotificationReadRequest(Guid notificationId)
    {
        NotificationId = notificationId;
    }
}

public class UpdateNotificationReadStatusRequestSpec : Specification<Notification>
{
    public UpdateNotificationReadStatusRequestSpec(Guid notificationId, string userId)
    {
        Query.Where(x => x.Id.Equals(notificationId) && x.ReceiverId.Equals(userId));
    }
}

public class UpdateNotificationReadRequestHandler : IRequestHandler<UpdateNotificationReadRequest, string>
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateNotificationReadRequestHandler(IRepository<Notification> notificationRepository, ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(UpdateNotificationReadRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        var spec = new UpdateNotificationReadStatusRequestSpec(request.NotificationId, userId);
        var notification = await _notificationRepository.FirstOrDefaultAsync(spec, cancellationToken) ?? throw new NotFoundException("Notification not found.");

        notification.UpdateIsRead();

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return "Notification read status updated successfully.";
    }
}

