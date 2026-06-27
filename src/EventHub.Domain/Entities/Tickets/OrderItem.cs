namespace EventHub.Domain.Entities.Tickets;

public sealed class OrderItem
{
    public Guid Id { get; init; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public TicketType TicketType { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}