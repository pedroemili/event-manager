using EventHub.Application.Events.Commands;
using EventHub.Application.Events.DTOs;
using EventHub.Application.Events.Queries;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventsController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<EventListResponse>>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] bool? featured = null,
        [FromQuery] bool upcoming = true,
        CancellationToken ct = default)
    {
        if (page < 1) throw new BadRequestException("page debe ser >= 1");
        if (pageSize < 1 || pageSize > 100)
            throw new BadRequestException("pageSize debe estar entre 1 y 100");

        var query = new GetPublishedEventsQuery(page, pageSize, search, categoryId, fromDate, featured, upcoming);
        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> GetBySlug(
        string slug,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEventBySlugQuery(slug), ct);
        if (result is null)
            throw new NotFoundException("Event", slug);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventListResponse>>>> GetMyEvents(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyEventsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEventByIdQuery(id), ct);
        if (result is null) throw new NotFoundException("Event", id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> Create(
        [FromBody] CreateEventCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> Update(
        Guid id,
        [FromBody] UpdateEventCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            throw new BadRequestException("El id de la URL no coincide con el del cuerpo.");

        var result = await Mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> Publish(
        Guid id,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new PublishEventCommand(id), ct);
        return Ok(result, "Evento publicado.");
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<EventDetailResponse>>> Cancel(
        Guid id,
        [FromBody] CancelEventRequest body,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelEventCommand(id, body.Reason), ct);
        return Ok(result, "Evento cancelado.");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        Guid id,
        CancellationToken ct)
    {
        await Mediator.Send(new DeleteEventCommand(id), ct);
        return NoContentResponse("Evento eliminado.");
    }

    [HttpPost("{id:guid}/favorite")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleFavorite(
        Guid id,
        CancellationToken ct)
    {
        var added = await Mediator.Send(new ToggleFavoriteCommand(id), ct);
        return Ok(added, added ? "Añadido a favoritos." : "Eliminado de favoritos.");
    }
}

public sealed record CancelEventRequest(string Reason);
