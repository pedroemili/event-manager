namespace EventHub.Domain.Entities.Audit;

public sealed class SystemLog
{
    public Guid Id { get; init; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}