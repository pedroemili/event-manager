using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasMaxLength(350)
            .IsRequired();

        builder.Property(e => e.Timezone)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("UTC");

        builder.Property(e => e.Status)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Draft");

        builder.Property(e => e.MainImageUrl)
            .HasMaxLength(512);

        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(512);

        builder.Property(e => e.CardImageUrl)
            .HasMaxLength(512);

        builder.Property(e => e.HeroImageUrl)
            .HasMaxLength(512);

        builder.HasIndex(e => e.Slug).IsUnique();

        builder.HasIndex(e => new { e.Status, e.StartDate })
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasIndex(e => new { e.OrganizerId, e.Status });
        builder.HasIndex(e => new { e.CategoryId, e.Status });
        builder.HasIndex(e => new { e.VenueId, e.Status });

        builder.HasOne(e => e.Organizer)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ScheduledPublication)
            .WithOne(sp => sp.Event)
            .HasForeignKey<ScheduledPublication>(sp => sp.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Images)
            .WithOne(ei => ei.Event)
            .HasForeignKey(ei => ei.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.EventTags)
            .WithOne(et => et.Event)
            .HasForeignKey(et => et.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.TicketTypes)
            .WithOne(tt => tt.Event)
            .HasForeignKey(tt => tt.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DiscountCodes)
            .WithOne(dc => dc.Event)
            .HasForeignKey(dc => dc.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.StaffList)
            .WithOne(es => es.Event)
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Tickets)
            .WithOne(t => t.Event)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}