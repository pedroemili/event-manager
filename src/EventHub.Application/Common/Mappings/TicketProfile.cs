using AutoMapper;
using EventHub.Application.Tickets.DTOs;
using EventHub.Domain.Entities.Tickets;

namespace EventHub.Application.Common.Mappings;

public sealed class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(d => d.OrderItems, o => o.MapFrom(s => MapOrderItems(s.OrderItems)));

        CreateMap<Ticket, TicketResponse>()
            .ForMember(d => d.EventTitle, o => o.MapFrom(s => s.Event == null ? null : s.Event.Title))
            .ForMember(d => d.EventStartDate, o => o.MapFrom(s => s.Event == null ? default : s.Event.StartDate))
            .ForMember(d => d.VenueName, o => o.MapFrom(s => s.Event == null || s.Event.Venue == null ? null : s.Event.Venue.Name))
            .ForMember(d => d.TicketTypeName, o => o.MapFrom(s => s.OrderItem == null ? null : s.OrderItem.TicketTypeName));
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
