using EventHub.Domain.Enums;

namespace EventHub.Application.Common;

/// <summary>
/// Centralized string conversion helpers between Domain enums and the
/// string-valued DB columns. The schema persists statuses/visibilities/
/// types as VARCHAR (with CHECK constraints) for portability and forward
/// compatibility. Use these helpers at every read/write site to avoid
/// magic strings and typo-induced bugs.
/// </summary>
public static class DomainStringExtensions
{
    public static string Value(this EventStatus status) => status.ToString();
    public static string Value(this TicketStatus status) => status.ToString();
    public static string Value(this OrderStatus status) => status.ToString();
    public static string Value(this EventVisibility visibility) => visibility.ToString();

    public static EventStatus ToEventStatus(this string value) =>
        Enum.TryParse<EventStatus>(value, ignoreCase: false, out var v)
            ? v
            : EventStatus.Draft;

    public static TicketStatus ToTicketStatus(this string value) =>
        Enum.TryParse<TicketStatus>(value, ignoreCase: false, out var v)
            ? v
            : TicketStatus.Active;

    public static OrderStatus ToOrderStatus(this string value) =>
        Enum.TryParse<OrderStatus>(value, ignoreCase: false, out var v)
            ? v
            : OrderStatus.Pending;

    public static EventVisibility ToEventVisibility(this string value) =>
        Enum.TryParse<EventVisibility>(value, ignoreCase: false, out var v)
            ? v
            : EventVisibility.Public;
}
