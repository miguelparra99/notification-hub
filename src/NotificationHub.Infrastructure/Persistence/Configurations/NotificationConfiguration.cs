using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Domain.Entities;

namespace NotificationHub.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Recipient).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(500);
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.Channel).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasMany(x => x.Attempts)
            .WithOne()
            .HasForeignKey(a => a.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Notification.Attempts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.Status, x.NextRetryAt });
        builder.HasIndex(x => x.CreatedAt);
    }
}