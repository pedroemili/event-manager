using EventHub.Domain.Entities.Notifications;
using EventHub.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(512);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.Timezone)
            .HasMaxLength(50);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.LastLoginAt);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.EmailVerificationTokens)
            .WithOne(et => et.User)
            .HasForeignKey(et => et.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PasswordResetTokens)
            .WithOne(pr => pr.User)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.LoginAttempts)
            .WithOne(la => la.User)
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.EventFavorites)
            .WithOne(ef => ef.User)
            .HasForeignKey(ef => ef.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.NotificationPreference)
            .WithOne(np => np.User)
            .HasForeignKey<NotificationPreference>(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}