
using ECO.WebApi.Domain.Notifications;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;
internal class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder
            .ToTable("Notifications", SchemaNames.Notification);

        builder
            .Property(b => b.Title)
            .HasMaxLength(256);

        builder
            .Property(b => b.Message)
            .HasMaxLength(2048);

        builder
            .Property(b => b.Label)
            .HasConversion<string>()
            .HasColumnType("varchar(50)");

        builder
            .Property(b => b.IsRead)
            .HasDefaultValue(false);

        builder
            .Property(b => b.Url)
            .HasMaxLength(2048);
    }
}
