using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class ScheduledPublicationConfiguration : IEntityTypeConfiguration<ScheduledPublication>
{
    public void Configure(EntityTypeBuilder<ScheduledPublication> builder)
    {
        builder.HasKey(sp => sp.Id);
        builder.HasIndex(sp => sp.EventId).IsUnique();
    }
}