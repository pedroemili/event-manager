using EventHub.Domain.Entities.Events;
using EventHub.Domain.Entities.Users;

namespace EventHub.Domain.Entities.Tickets;

public sealed class Order
{
    public Guid Id { get; init; }
    public Guid UserId { get; set; }
    public Guid? DiscountCodeId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Pending";
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public DiscountCode? DiscountCode { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}