using EventHub.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.HasKey(la => la.Id);

        builder.Property(la => la.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(la => la.IpAddress)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(la => la.FailureReason)
            .HasMaxLength(100);

        builder.HasIndex(la => new { la.UserId, la.AttemptedAt });
        builder.HasIndex(la => new { la.IpAddress, la.AttemptedAt });
    }
}