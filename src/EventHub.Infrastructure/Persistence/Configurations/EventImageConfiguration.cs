using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EventImageConfiguration : IEntityTypeConfiguration<EventImage>
{
    public void Configure(EntityTypeBuilder<EventImage> builder)
    {
        builder.HasKey(ei => ei.Id);

        builder.Property(ei => ei.ImageUrl)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(ei => ei.AltText)
            .HasMaxLength(200);

        builder.HasIndex(ei => new { ei.EventId, ei.OrderIndex });
    }
}