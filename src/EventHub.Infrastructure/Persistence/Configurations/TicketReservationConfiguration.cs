using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class TicketReservationConfiguration : IEntityTypeConfiguration<TicketReservation>
{
    public void Configure(EntityTypeBuilder<TicketReservation> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.HasIndex(tr => tr.ExpiresAt)
            .HasFilter("\"IsConfirmed\" = false");

        builder.HasIndex(tr => new { tr.TicketTypeId, tr.IsConfirmed });

        builder.HasOne(tr => tr.User)
            .WithMany()
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.Order)
            .WithMany()
            .HasForeignKey(tr => tr.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}