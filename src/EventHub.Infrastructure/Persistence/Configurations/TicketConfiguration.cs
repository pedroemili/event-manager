using EventHub.Domain.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasMaxLength(15)
            .IsRequired()
            .HasDefaultValue("Active");

        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.HasIndex(t => t.QrCodeData).IsUnique();

        builder.HasIndex(t => new { t.UserId, t.Status, t.CreatedAt });
        builder.HasIndex(t => new { t.EventId, t.Status, t.CreatedAt });
        builder.HasIndex(t => new { t.EventId, t.Status });

        builder.Property(t => t.CheckInMethod)
            .HasMaxLength(50);

        builder.Property(t => t.CheckInIpAddress)
            .HasMaxLength(45);

        builder.HasOne(t => t.User)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CheckedInByUser)
            .WithMany()
            .HasForeignKey(t => t.CheckedInBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}