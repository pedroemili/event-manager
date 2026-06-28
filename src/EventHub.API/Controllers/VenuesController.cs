using EventHub.Application.Common.Interfaces;
using EventHub.Application.Venues.DTOs;
using EventHub.Application.Venues.Queries;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VenuesController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<VenueResponse>>>> List(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new ListVenuesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<VenueResponse>>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetVenueByIdQuery(id), ct);
        if (result is null) throw new EventHub.Shared.Exceptions.NotFoundException("Venue", id);
        return Ok(result);
    }
}
