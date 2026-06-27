using EventHub.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.HasKey(el => el.Id);

        builder.Property(el => el.ToEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(el => el.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(el => el.TemplateType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(el => el.Status)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder.HasIndex(el => el.UserId);
        builder.HasIndex(el => el.Status);
    }
}