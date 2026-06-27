using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Tickets;

public sealed class TicketReservation
{
    public Guid Id { get; init; }
    public Guid TicketTypeId { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; }
    public Guid? OrderId { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public TicketType TicketType { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order? Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}