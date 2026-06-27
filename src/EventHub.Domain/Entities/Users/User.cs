using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Notifications;

using EventHub.Domain.Entities.Staff;
using EventHub.Domain.Entities.Tickets;

namespace EventHub.Domain.Entities.Users;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;
    public DateTime UpdatedAt { get; set; } = TimeProvider.System.GetUtcNow().UtcDateTime;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public NotificationPreference? NotificationPreference { get; set; }
    public ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    public ICollection<EventStaff> EventStaffAssignments { get; set; } = new List<EventStaff>();
}