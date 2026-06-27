using EventHub.Domain.Entities.Audit;
using EventHub.Domain.Entities.Auth;
using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Notifications;
using EventHub.Domain.Entities.Staff;
using EventHub.Domain.Entities.Tickets;
using EventHub.Domain.Entities.Users;
using EventHub.Domain.Entities.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class EventHubDbContext : DbContext
{
    public EventHubDbContext(DbContextOptions<EventHubDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<EventTag> EventTags => Set<EventTag>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventImage> EventImages => Set<EventImage>();
    public DbSet<ScheduledPublication> ScheduledPublications => Set<ScheduledPublication>();
    public DbSet<EventFavorite> EventFavorites => Set<EventFavorite>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketReservation> TicketReservations => Set<TicketReservation>();

    public DbSet<EventStaff> EventStaff => Set<EventStaff>();

    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventHubDbContext).Assembly);
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private static void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Venue>().HasQueryFilter(v => v.DeletedAt == null);
    }
}