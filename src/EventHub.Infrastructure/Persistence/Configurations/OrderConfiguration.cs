using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .HasMaxLength(30)
            .IsRequired()
            .HasDefaultValueSql("'ORD-' || TO_CHAR(NOW() AT TIME ZONE 'UTC', 'YYYYMMDD') || '-' || LPAD(NEXTVAL('order_number_seq')::TEXT, 6, '0')");

        builder.Property(o => o.SubtotalAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.TaxAmount)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property(o => o.Status)
            .HasMaxLength(15)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder.Property(o => o.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(o => o.PaymentTransactionId)
            .HasMaxLength(100);

        builder.Property(o => o.PaymentStatus)
            .HasMaxLength(20);

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => new { o.UserId, o.Status, o.CreatedAt });
        builder.HasIndex(o => new { o.Status, o.CreatedAt });

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.DiscountCode)
            .WithMany(dc => dc.Orders)
            .HasForeignKey(o => o.DiscountCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Tickets)
            .WithOne(t => t.Order)
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}