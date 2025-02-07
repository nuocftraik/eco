
using System;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Application.Notifications;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Domain.Notifications;
using ECO.WebApi.Shared.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Notifications;
public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationSender _notificationSender;
    private readonly IJobService _jobService;

    public NotificationService(IRepository<Notification> notificationRepository, UserManager<ApplicationUser> userManager, INotificationSender notificationSender, IJobService jobService)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
        _notificationSender = notificationSender;
        _jobService = jobService;
    }
    public async Task SendNotificationToAllUsers(BasicNotification notification, CancellationToken cancellationToken)
    {
         _jobService.Enqueue(() => ExcuteSendNotificationToAllUsers(notification, cancellationToken));

    }

    public async Task SendNotificationToUser(BasicNotification notification, string userId, CancellationToken cancellationToken)
    {
        _jobService.Enqueue(() => ExcuteSendNotificationToUser(userId, notification, cancellationToken));
    }

    public async Task SendNotificationToUsers(BasicNotification notification, List<string> userIds, CancellationToken cancellationToken)
    {
        _jobService.Enqueue(() => ExcuteSendNotificationToUsers(userIds, notification, cancellationToken));
    }

    public async Task ExcuteSendNotificationToAllUsers(BasicNotification notification, CancellationToken cancellationToken)
    {
        List<string> userIds = await _userManager.Users.Select(u => u.Id).ToListAsync(cancellationToken);
        List<Notification> addNotis = new List<Notification>();
        foreach (string userId in userIds)
        {
            addNotis.Add(new Notification(userId, notification.Title ,notification.Label, notification.Message, notification.Url));
        }

        await _notificationRepository.AddRangeAsync(addNotis, cancellationToken);
        await _notificationSender.SendToUsersAsync(notification, userIds, cancellationToken);
    }

    public async Task ExcuteSendNotificationToUser(string userId, BasicNotification notification, CancellationToken cancellationToken)
    {
        Notification addNoti = new Notification(userId,notification.Title,notification.Label,notification.Message,notification.Url);
        await _notificationRepository.AddAsync(addNoti, cancellationToken);
        await _notificationSender.SendToUserAsync(notification, userId, cancellationToken);
    }
    public async Task ExcuteSendNotificationToUsers(List<string> userIds, BasicNotification notification, CancellationToken cancellationToken)
    {
        List<Notification> addNotis = new List<Notification>();
        foreach (string userId in userIds)
        {
            addNotis.Add(new Notification(userId,notification.Title,notification.Label,notification.Message,notification.Url));
        }

        await _notificationRepository.AddRangeAsync(addNotis, cancellationToken);
        await _notificationSender.SendToUsersAsync(notification, userIds, cancellationToken);
    }
}
