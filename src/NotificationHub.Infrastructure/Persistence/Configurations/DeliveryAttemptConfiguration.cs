using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationHub.Domain.Entities;

namespace NotificationHub.Infrastructure.Persistence.Configurations;

public class DeliveryAttemptConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("DeliveryAttempts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);

        builder.HasIndex(x => x.NotificationId);
    }
}