using EventHub.Application.Common.Interfaces.Services;
using EventHub.Application.Common.Interfaces;
using EventHub.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ISender Mediator => HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ICurrentUserService CurrentUser => HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

    protected ActionResult<ApiResponse<T>> Ok<T>(T data, string? message = null)
    {
        return base.Ok(ApiResponse<T>.Ok(data, message));
    }

    protected ActionResult<ApiResponse<T>> Created<T>(T data, string? message = null)
    {
        return StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));
    }

    protected ActionResult<ApiResponse<object>> NoContentResponse(string? message = null)
    {
        return base.Ok(ApiResponse<object>.Ok(new { }, message ?? "Operation completed"));
    }
}
