using EventHub.Application.Auth.Commands;
using EventHub.Application.Auth.DTOs;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProfileController : ApiControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetMe(CancellationToken ct)
    {
        var userId = CurrentUser.UserId ?? throw new UnauthorizedException();
        var result = await Mediator.Send(new GetProfileQuery(userId), ct);
        return Ok(result);
    }
}
