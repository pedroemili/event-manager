using EventHub.Application.Dashboard.Queries;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController : ApiControllerBase
{
    [HttpGet("organizer/metrics")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<ActionResult<ApiResponse<DashboardMetricsResponse>>> OrganizerMetrics(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrganizerMetricsQuery(), ct);
        return Ok(result);
    }
}
