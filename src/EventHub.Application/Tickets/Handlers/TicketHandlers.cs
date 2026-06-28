using EventHub.Application.Common.Interfaces;
using EventHub.Application.Common.Interfaces.Services;
using EventHub.Application.Tickets.Commands;
using EventHub.Application.Tickets.DTOs;
using EventHub.Domain.Entities.Tickets;
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
        if (ticketType.SoldQuantity + cmd.Quantity > ticketType.TotalQuantity)
            throw new BadRequestException("No hay suficientes boletos disponibles.");

        var reservation = new TicketReservation
        {
            TicketTypeId = cmd.TicketTypeId,
            UserId = userId,
            Quantity = cmd.Quantity,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            ReservationCode = $"RSV-{Random.Shared.Next(100000, 999999)}"
        };

        await _reservationRepo.AddAsync(reservation, ct);
        await _uow.SaveChangesAsync(ct);

        return new ReservationResponse(reservation.Id, reservation.ReservationCode, reservation.ExpiresAt);
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

        var order = new Order
        {
            UserId = reservation.UserId,
            SubtotalAmount = subtotal,
            TotalAmount = subtotal,
            Status = "Confirmed",
            ConfirmedAt = DateTime.UtcNow
        };

        var orderItem = new OrderItem
        {
            TicketTypeId = ticketType.Id,
            TicketTypeName = ticketType.Name,
            Quantity = reservation.Quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal
        };

        for (int i = 0; i < reservation.Quantity; i++)
        {
            var ticket = new Ticket
            {
                UserId = reservation.UserId,
                EventId = ticketType.EventId,
                TicketNumber = TicketNumberGenerator.Generate(ticketType.EventId.ToString("N")[..6], ticketType.SoldQuantity + 1),
                QrCodeData = _qrService.GenerateQrData(Guid.NewGuid(), ticketType.EventId, reservation.UserId)
            };
            orderItem.Tickets.Add(ticket);
        }

        order.OrderItems.Add(orderItem);
        ticketType.SoldQuantity += reservation.Quantity;

        reservation.IsConfirmed = true;
        reservation.ConfirmedAt = DateTime.UtcNow;
        reservation.OrderId = order.Id;

        await _orderRepo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return new OrderResponse(
            order.Id, order.OrderNumber ?? OrderNumberGenerator.Generate(),
            order.SubtotalAmount, order.DiscountAmount, order.TaxAmount,
            order.TotalAmount, order.Currency, order.Status,
            order.PaymentMethod, order.PaymentStatus, order.CreatedAt,
            order.OrderItems.Select(oi => new OrderItemResponse(
                oi.Id, oi.TicketTypeName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
                oi.Tickets.Select(t => new TicketResponse(
                    t.Id, t.TicketNumber, t.QrCodeData, t.QrCodeImageUrl,
                    t.Status, t.CheckedInAt, null, DateTime.MinValue, null, null
                )).ToList()
            )).ToList()
        );
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

        var ticket = await _repo.GetByQrDataAsync(cmd.QrData, ct);
        if (ticket is null) throw new NotFoundException(nameof(Ticket), decoded.Value.ticketId);

        if (ticket.Status != "Active") throw new BadRequestException($"Este boleto ya fue {ticket.Status.ToLowerInvariant()}.");
        if (ticket.EventId != cmd.EventId) throw new BadRequestException("Este boleto no pertenece a este evento.");

        ticket.Status = "Used";
        ticket.CheckedInAt = DateTime.UtcNow;
        ticket.CheckedInBy = staffId;
        ticket.CheckInMethod = "QR_SCAN";
        await _uow.SaveChangesAsync(ct);

        return new TicketResponse(
            ticket.Id, ticket.TicketNumber, ticket.QrCodeData, ticket.QrCodeImageUrl,
            ticket.Status, ticket.CheckedInAt, ticket.Event.Title, ticket.Event.StartDate,
            ticket.Event.Venue?.Name, ticket.OrderItem.TicketTypeName
        );
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
        return tickets.Select(t => new TicketResponse(
            t.Id, t.TicketNumber, t.QrCodeData, t.QrCodeImageUrl,
            t.Status, t.CheckedInAt, t.Event.Title, t.Event.StartDate,
            t.Event.Venue?.Name, t.OrderItem.TicketTypeName
        )).ToList();
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
        return orders.Select(o => new OrderResponse(
            o.Id, o.OrderNumber, o.SubtotalAmount, o.DiscountAmount, o.TaxAmount,
            o.TotalAmount, o.Currency, o.Status, o.PaymentMethod, o.PaymentStatus, o.CreatedAt,
            o.OrderItems.Select(oi => new OrderItemResponse(
                oi.Id, oi.TicketTypeName, oi.Quantity, oi.UnitPrice, oi.Subtotal,
                oi.Tickets.Select(t => new TicketResponse(
                    t.Id, t.TicketNumber, t.QrCodeData, t.QrCodeImageUrl,
                    t.Status, t.CheckedInAt, null, DateTime.MinValue, null, null
                )).ToList()
            )).ToList()
        )).ToList();
    }
}