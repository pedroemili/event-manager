using AutoMapper;
using EventHub.Application.Tickets.DTOs;
using EventHub.Domain.Entities.Tickets;

namespace EventHub.Application.Common.Mappings;

public sealed class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ConstructUsing(src => new OrderResponse(
                src.Id, src.OrderNumber, src.SubtotalAmount, src.DiscountAmount, src.TaxAmount,
                src.TotalAmount, src.Currency, src.Status, src.PaymentMethod, src.PaymentStatus, src.CreatedAt,
                MapOrderItems(src.OrderItems)))
            .ForMember(d => d.OrderItems, o => o.Ignore());

        CreateMap<Ticket, TicketResponse>()
            .ConstructUsing(src => new TicketResponse(
                src.Id, src.TicketNumber, src.QrCodeData, src.QrCodeImageUrl,
                src.Status, src.CheckedInAt,
                src.Event == null ? null : src.Event.Title,
                src.Event == null ? default : src.Event.StartDate,
                src.Event == null || src.Event.Venue == null ? null : src.Event.Venue.Name,
                src.OrderItem == null ? null : src.OrderItem.TicketTypeName))
            .ForMember(d => d.EventTitle, o => o.Ignore())
            .ForMember(d => d.EventStartDate, o => o.Ignore())
            .ForMember(d => d.VenueName, o => o.Ignore())
            .ForMember(d => d.TicketTypeName, o => o.Ignore());
    }

    private static IReadOnlyList<OrderItemResponse> MapOrderItems(ICollection<OrderItem> orderItems)
    {
        if (orderItems == null) return Array.Empty<OrderItemResponse>();
        return orderItems.Select(oi => new OrderItemResponse(
            oi.Id, oi.TicketTypeName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
            MapTickets(oi.Tickets))).ToList();
    }

    private static List<TicketResponse> MapTickets(ICollection<Ticket> tickets)
    {
        if (tickets == null) return new List<TicketResponse>();
        return tickets.Select(t => new TicketResponse(
            t.Id, t.TicketNumber, t.QrCodeData, t.QrCodeImageUrl,
            t.Status, t.CheckedInAt, null, DateTime.MinValue, null, null)).ToList();
    }
}
