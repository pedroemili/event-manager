using EventHub.Application.Tickets.Commands;
using EventHub.Application.Tickets.DTOs;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ApiControllerBase
{
    [HttpPost("reservations")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ReservationResponse>>> CreateReservation(
        [FromBody] CreateReservationCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Created(result);
    }

    [HttpPost("orders/confirm")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> ConfirmOrder(
        [FromBody] ConfirmOrderCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Created(result);
    }

    [HttpPost("orders/{id:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> CancelOrder(
        Guid id,
        CancellationToken ct)
    {
        await Mediator.Send(new CancelOrderCommand(id), ct);
        return NoContentResponse("Orden cancelada.");
    }

    [HttpPost("validate")]
    [Authorize(Roles = "Staff,Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<TicketResponse>>> Validate(
        [FromBody] ValidateTicketBody body,
        CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await Mediator.Send(
            new ValidateTicketCommand(body.QrData, body.EventId, ip),
            ct);
        return Ok(result, "Ticket validado.");
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TicketResponse>>>> GetMyTickets(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyTicketsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("orders")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderResponse>>>> GetMyOrders(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyOrdersQuery(), ct);
        return Ok(result);
    }
}

public sealed record ValidateTicketBody(string QrData, Guid EventId);
