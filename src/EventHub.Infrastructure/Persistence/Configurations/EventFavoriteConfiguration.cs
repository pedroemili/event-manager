using EventHub.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EventFavoriteConfiguration : IEntityTypeConfiguration<EventFavorite>
{
    public void Configure(EntityTypeBuilder<EventFavorite> builder)
    {
        builder.HasKey(ef => new { ef.UserId, ef.EventId });

        builder.HasOne(ef => ef.User)
            .WithMany(u => u.EventFavorites)
            .HasForeignKey(ef => ef.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ef => ef.Event)
            .WithMany(e => e.EventFavorites)
            .HasForeignKey(ef => ef.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}