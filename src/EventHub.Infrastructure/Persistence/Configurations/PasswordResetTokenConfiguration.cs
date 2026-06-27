using EventHub.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Token)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(pr => pr.Token).IsUnique();
    }
}