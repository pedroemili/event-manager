using EventHub.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(al => al.EntityType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(al => al.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.CorrelationId)
            .HasMaxLength(100);

        builder.Property(al => al.OldValues)
            .HasColumnType("jsonb");

        builder.Property(al => al.NewValues)
            .HasColumnType("jsonb");

        builder.HasIndex(al => new { al.EntityType, al.EntityId, al.CreatedAt });
        builder.HasIndex(al => new { al.UserId, al.CreatedAt });
        builder.HasIndex(al => new { al.Action, al.CreatedAt });
        builder.HasIndex(al => al.CreatedAt);
    }
}