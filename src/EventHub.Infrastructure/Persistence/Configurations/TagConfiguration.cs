using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}