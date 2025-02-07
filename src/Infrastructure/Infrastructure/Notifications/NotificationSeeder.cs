
using System.Reflection;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Notifications;
using ECO.WebApi.Infrastructure.Persistence.Context;
using ECO.WebApi.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.WebApi.Infrastructure.Notifications;
public class NotificationSeeder : ICustomSeeder
{
    private readonly ISerializerService _serializerService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<NotificationSeeder> _logger;

    public NotificationSeeder(ISerializerService serializerService, ApplicationDbContext db, ILogger<NotificationSeeder> logger)
    {
        _serializerService = serializerService;
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string dataPath = Path.Combine(path!, "Notifications", "notificationData.json");
        if (!_db.Notifications.Any())
        {
            _logger.LogInformation("Started to Seed Notifications.");
            string notificationData = await File.ReadAllTextAsync(dataPath, cancellationToken);
            var notifications = _serializerService.Deserialize<List<Notification>>(notificationData);
            var user = await _db.Users.Where(u => u.UserName == "system.admin").FirstOrDefaultAsync();
            foreach (var notification in notifications)
            {
                notification.ReceiverId = user.Id;
                _ = _db.Notifications.Add(notification);
            }

            _ = await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded Notifications.");
        }

    }
}

