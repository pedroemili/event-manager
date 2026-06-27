using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EventFavoriteConfiguration : IEntityTypeConfiguration<EventFavorite>
{
    public void Configure(EntityTypeBuilder<EventFavorite> builder)
    {
        builder.HasKey(ef => new { ef.UserId, ef.EventId });
    }
}