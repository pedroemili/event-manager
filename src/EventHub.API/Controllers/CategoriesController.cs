using EventHub.Application.Categories.Queries;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventHub.Application.Categories.DTOs.CategoryResponse>>>> List(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetActiveCategoriesQuery(), ct);
        return Ok(result);
    }
}
