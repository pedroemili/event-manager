using EventHub.Domain.Entities.Events;

namespace EventHub.Domain.Entities.Tickets;

public sealed class TicketType
{
    public Guid Id { get; init; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int MinPerOrder { get; set; } = 1;
    public int MaxPerOrder { get; set; } = 10;
    public string Type { get; set; } = "Standard";
    public DateTime? SalesStartAt { get; set; }
    public DateTime? SalesEndAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<TicketReservation> Reservations { get; set; } = new List<TicketReservation>();
}