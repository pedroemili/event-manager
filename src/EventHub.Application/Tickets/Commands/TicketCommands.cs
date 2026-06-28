using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Application.Tickets.DTOs;
using EventHub.Domain.Entities.Tickets;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Helpers;
using MediatR;

namespace EventHub.Application.Tickets.Commands;

public sealed record CreateReservationCommand(Guid TicketTypeId, int Quantity) : IRequest<ReservationResponse>;

public sealed record ConfirmOrderCommand(Guid ReservationId, string? DiscountCode) : IRequest<OrderResponse>;

public sealed record ValidateTicketCommand(string QrData, Guid EventId) : IRequest<TicketResponse>;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest;

public sealed record GetMyTicketsQuery : IRequest<IReadOnlyList<TicketResponse>>;

public sealed record GetMyOrdersQuery : IRequest<IReadOnlyList<OrderResponse>>;