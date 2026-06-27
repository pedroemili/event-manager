using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.HasKey(dc => dc.Id);

        builder.Property(dc => dc.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(dc => dc.Description)
            .HasMaxLength(300);

        builder.Property(dc => dc.Value)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(dc => dc.Type)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(dc => dc.MinPurchaseAmount)
            .HasColumnType("decimal(10,2)");

        builder.HasIndex(dc => new { dc.Code, dc.EventId }).IsUnique();
        builder.HasIndex(dc => new { dc.EventId, dc.IsActive });
    }
}