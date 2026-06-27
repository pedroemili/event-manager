using EventHub.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(512);

        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}