using EventHub.Domain.Entities.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EventStaffConfiguration : IEntityTypeConfiguration<EventStaff>
{
    public void Configure(EntityTypeBuilder<EventStaff> builder)
    {
        builder.HasKey(es => es.Id);

        builder.Property(es => es.StaffRole)
            .HasMaxLength(50);

        builder.Property(es => es.Status)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("Pending");

        builder.HasIndex(es => new { es.EventId, es.UserId }).IsUnique();
        builder.HasIndex(es => es.UserId);

        builder.HasOne(es => es.Inviter)
            .WithMany()
            .HasForeignKey(es => es.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}