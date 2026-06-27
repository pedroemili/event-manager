using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(oi => oi.TicketTypeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(oi => oi.Subtotal)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.TicketTypeId);

        builder.HasMany(oi => oi.Tickets)
            .WithOne(t => t.OrderItem)
            .HasForeignKey(t => t.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}