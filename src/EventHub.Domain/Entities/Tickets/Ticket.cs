using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Tickets;

public sealed class Ticket
{
    public Guid Id { get; init; }
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string QrCodeData { get; set; } = string.Empty;
    public string? QrCodeImageUrl { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? CheckedInAt { get; set; }
    public Guid? CheckedInBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public OrderItem OrderItem { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? CheckedInByUser { get; set; }
}