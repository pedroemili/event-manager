using EventHub.Domain.Entities.Events;

namespace EventHub.Domain.Entities.Tickets;

public sealed class DiscountCode
{
    public Guid Id { get; init; }
    public Guid EventId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int MaxTotalUses { get; set; }
    public int CurrentUses { get; set; }
    public int MaxUsesPerUser { get; set; } = 1;
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}