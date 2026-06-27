using EventHub.Domain.Entities.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Address)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(v => v.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.State)
            .HasMaxLength(100);

        builder.Property(v => v.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.ZipCode)
            .HasMaxLength(20);

        builder.Property(v => v.Latitude)
            .HasColumnType("decimal(9,6)");

        builder.Property(v => v.Longitude)
            .HasColumnType("decimal(9,6)");

        builder.HasIndex(v => v.CreatedBy);
        builder.HasIndex(v => new { v.City, v.Country });

        builder.HasOne(v => v.Creator)
            .WithMany()
            .HasForeignKey(v => v.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Events)
            .WithOne(e => e.Venue)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}