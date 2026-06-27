using EventHub.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasKey(et => et.Id);

        builder.Property(et => et.Token)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(et => et.Token).IsUnique();
    }
}