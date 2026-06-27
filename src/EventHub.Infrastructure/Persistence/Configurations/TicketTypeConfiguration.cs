using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tt => tt.Price)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(tt => tt.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property(tt => tt.Type)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Standard");

        builder.HasIndex(tt => new { tt.EventId, tt.IsActive });

        builder.HasMany(tt => tt.OrderItems)
            .WithOne(oi => oi.TicketType)
            .HasForeignKey(oi => oi.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(tt => tt.Reservations)
            .WithOne(tr => tr.TicketType)
            .HasForeignKey(tr => tr.TicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}