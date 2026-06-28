using System.Security.Cryptography;
using EventHub.Application.Common;
using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Application.Tickets.Commands;
using EventHub.Application.Tickets.DTOs;
using EventHub.Domain.Entities.Tickets;
using EventHub.Domain.Enums;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Helpers;
using MediatR;

namespace EventHub.Application.Tickets.Handlers;

public sealed class CreateReservationHandler : IRequestHandler<CreateReservationCommand, ReservationResponse>
{
    private readonly IRepository<TicketType> _ticketTypeRepo;
    private readonly IRepository<TicketReservation> _reservationRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateReservationHandler(
        IRepository<TicketType> ticketTypeRepo,
        IRepository<TicketReservation> reservationRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _ticketTypeRepo = ticketTypeRepo;
        _reservationRepo = reservationRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ReservationResponse> Handle(CreateReservationCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var ticketType = await _ticketTypeRepo.GetByIdAsync(cmd.TicketTypeId, ct);
        if (ticketType is null) throw new NotFoundException(nameof(TicketType), cmd.TicketTypeId);
        if (ticketType.TotalQuantity - ticketType.SoldQuantity < cmd.Quantity)
            throw new BadRequestException("No hay suficientes boletos disponibles.");

        var reservation = new TicketReservation
        {
            TicketTypeId = cmd.TicketTypeId,
            UserId = userId,
            Quantity = cmd.Quantity,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            ReservationCode = $"RSV-{GenerateReservationCode()}"
        };

        await _reservationRepo.AddAsync(reservation, ct);
        await _uow.SaveChangesAsync(ct);

        return new ReservationResponse(reservation.Id, reservation.ReservationCode, reservation.ExpiresAt);
    }

    private static string GenerateReservationCode()
    {
        Span<byte> bytes = stackalloc byte[6];
        RandomNumberGenerator.Fill(bytes);
        var n = BitConverter.ToUInt32(bytes) % 900_000 + 100_000;
        return n.ToString();
    }
}

public sealed class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand, OrderResponse>
{
    private readonly IRepository<TicketReservation> _reservationRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IRepository<TicketType> _ticketTypeRepo;
    private readonly IQRCodeService _qrService;
    private readonly IUnitOfWork _uow;

    public ConfirmOrderHandler(
        IRepository<TicketReservation> reservationRepo,
        IOrderRepository orderRepo,
        IRepository<TicketType> ticketTypeRepo,
        IQRCodeService qrService,
        IUnitOfWork uow)
    {
        _reservationRepo = reservationRepo;
        _orderRepo = orderRepo;
        _ticketTypeRepo = ticketTypeRepo;
        _qrService = qrService;
        _uow = uow;
    }

    public async Task<OrderResponse> Handle(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var reservation = await _reservationRepo.GetByIdAsync(cmd.ReservationId, ct);
        if (reservation is null) throw new NotFoundException(nameof(TicketReservation), cmd.ReservationId);
        if (reservation.ExpiresAt < DateTime.UtcNow) throw new BadRequestException("La reserva ha expirado.");
        if (reservation.IsConfirmed) throw new BadRequestException("La reserva ya fue confirmada.");

        var ticketType = await _ticketTypeRepo.GetByIdAsync(reservation.TicketTypeId, ct);
        if (ticketType is null) throw new NotFoundException(nameof(TicketType), reservation.TicketTypeId);

        var unitPrice = ticketType.Price;
        var subtotal = unitPrice * reservation.Quantity;

        // Step 1: persist Order alone so we get its DB-assigned Id.
        var order = new Order
        {
            UserId = reservation.UserId,
            OrderNumber = OrderNumberGenerator.Generate(),
            SubtotalAmount = subtotal,
            TotalAmount = subtotal,
            Status = OrderStatus.Confirmed.Value(),
            ConfirmedAt = DateTime.UtcNow
        };
        await _orderRepo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        // Step 2: build OrderItems and Tickets now that we have the Order.Id.
        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            TicketTypeId = ticketType.Id,
            TicketTypeName = ticketType.Name,
            Quantity = reservation.Quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal
        };
        order.OrderItems.Add(orderItem);

        var sequenceStart = ticketType.SoldQuantity;
        for (int i = 0; i < reservation.Quantity; i++)
        {
            var ticketNumber = TicketNumberGenerator.Generate(
                ticketType.EventId.ToString("N")[..6],
                sequenceStart + i + 1);
            var ticket = new Ticket
            {
                UserId = reservation.UserId,
                EventId = ticketType.EventId,
                OrderId = order.Id,
                OrderItemId = orderItem.Id,
                TicketNumber = ticketNumber,
                QrCodeData = _qrService.GenerateQrData(Guid.NewGuid(), ticketType.EventId, reservation.UserId)
            };
            orderItem.Tickets.Add(ticket);
        }
        ticketType.SoldQuantity += reservation.Quantity;

        // Step 3: update TicketReservation to point to the order.
        reservation.IsConfirmed = true;
        reservation.ConfirmedAt = DateTime.UtcNow;
        reservation.OrderId = order.Id;

        await _uow.SaveChangesAsync(ct);

        return TicketMapper.ToOrderResponse(order);
    }
}

public sealed class ValidateTicketHandler : IRequestHandler<ValidateTicketCommand, TicketResponse>
{
    private readonly ITicketRepository _repo;
    private readonly IQRCodeService _qrService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ValidateTicketHandler(ITicketRepository repo, IQRCodeService qrService, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _repo = repo;
        _qrService = qrService;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TicketResponse> Handle(ValidateTicketCommand cmd, CancellationToken ct)
    {
        var staffId = _currentUser.UserId ?? throw new UnauthorizedException();
        var decoded = _qrService.DecodeQrData(cmd.QrData);
        if (decoded is null) throw new BadRequestException("QR inválido.");

        var ticket = await _repo.GetByQrDataForUpdateAsync(cmd.QrData, ct)
            ?? throw new NotFoundException(nameof(Ticket), decoded.Value.ticketId);

        if (ticket.Status.ToTicketStatus() != TicketStatus.Active)
            throw new BadRequestException($"Este boleto ya fue {ticket.Status.ToLowerInvariant()}.");
        if (ticket.EventId != cmd.EventId)
            throw new BadRequestException("Este boleto no pertenece a este evento.");

        ticket.Status = TicketStatus.Used.Value();
        ticket.CheckedInAt = DateTime.UtcNow;
        ticket.CheckedInBy = staffId;
        ticket.CheckInMethod = "QR_SCAN";
        ticket.CheckInIpAddress = cmd.IpAddress;
        await _uow.SaveChangesAsync(ct);

        return TicketMapper.ToTicketResponse(ticket);
    }
}

public sealed class GetMyTicketsHandler : IRequestHandler<GetMyTicketsQuery, IReadOnlyList<TicketResponse>>
{
    private readonly ITicketRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetMyTicketsHandler(ITicketRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TicketResponse>> Handle(GetMyTicketsQuery q, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var tickets = await _repo.GetByUserIdAsync(userId, ct);
        return tickets.Select(TicketMapper.ToTicketResponse).ToList();
    }
}

public sealed class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderResponse>>
{
    private readonly IOrderRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetMyOrdersHandler(IOrderRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<OrderResponse>> Handle(GetMyOrdersQuery q, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var orders = await _repo.GetByUserIdAsync(userId, ct);
        return orders.Select(TicketMapper.ToOrderResponse).ToList();
    }
}

/// <summary>
/// Centralizes Ticket/Order -> DTO mapping. Removes the inline mapping
/// duplication that previously lived in 4 different handlers.
/// </summary>
internal static class TicketMapper
{
    public static TicketResponse ToTicketResponse(Ticket t) => new(
        t.Id, t.TicketNumber, t.QrCodeData, t.QrCodeImageUrl,
        t.Status, t.CheckedInAt,
        t.Event?.Title,
        t.Event?.StartDate ?? default,
        t.Event?.Venue?.Name,
        t.OrderItem?.TicketTypeName);

    public static OrderResponse ToOrderResponse(Order o) => new(
        o.Id, o.OrderNumber, o.SubtotalAmount, o.DiscountAmount, o.TaxAmount,
        o.TotalAmount, o.Currency, o.Status, o.PaymentMethod, o.PaymentStatus, o.CreatedAt,
        o.OrderItems?.Select(oi => new OrderItemResponse(
            oi.Id, oi.TicketTypeName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
            oi.Tickets?.Select(ToTicketResponse).ToList() ?? new List<TicketResponse>())).ToList()
        ?? new List<OrderItemResponse>());
}
