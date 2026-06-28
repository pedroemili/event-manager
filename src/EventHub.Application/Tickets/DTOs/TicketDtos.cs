namespace EventHub.Application.Tickets.DTOs;

public sealed record ReservationResponse(Guid Id, string ReservationCode, DateTime ExpiresAt);

public sealed record OrderResponse(
    Guid Id,
    string OrderNumber,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string Status,
    string? PaymentMethod,
    string? PaymentStatus,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemResponse> OrderItems
);

public sealed record OrderItemResponse(
    Guid Id,
    string TicketTypeName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    IReadOnlyList<TicketResponse> Tickets
);

public sealed record TicketResponse(
    Guid Id,
    string TicketNumber,
    string QrCodeData,
    string? QrCodeImageUrl,
    string Status,
    DateTime? CheckedInAt,
    string? EventTitle,
    DateTime EventStartDate,
    string? VenueName,
    string? TicketTypeName
);