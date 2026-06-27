using EventHub.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Level)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(sl => sl.Source)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sl => sl.RequestPath)
            .HasMaxLength(500);

        builder.Property(sl => sl.RequestMethod)
            .HasMaxLength(10);

        builder.Property(sl => sl.IpAddress)
            .HasMaxLength(45);

        builder.Property(sl => sl.CorrelationId)
            .HasMaxLength(100);

        builder.Property(sl => sl.AdditionalData)
            .HasColumnType("jsonb");

        builder.HasIndex(sl => new { sl.Level, sl.CreatedAt });
        builder.HasIndex(sl => new { sl.RequestPath, sl.CreatedAt });
        builder.HasIndex(sl => sl.CreatedAt);
        builder.HasIndex(sl => sl.CorrelationId);
    }
}