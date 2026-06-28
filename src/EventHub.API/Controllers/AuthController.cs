using EventHub.Application.Auth.Commands;
using EventHub.Application.Auth.DTOs;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password), cancellationToken);
        return NoContentResponse("Cuenta creada. Revisa tu email para verificarla.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await Mediator.Send(
            new LoginCommand(request.Email, request.Password, ip),
            cancellationToken);
        return Ok(result, "Inicio de sesión exitoso.");
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await Mediator.Send(
            new RefreshTokenCommand(request.RefreshToken, ip),
            cancellationToken);
        return Ok(result, "Token refrescado.");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return NoContentResponse("Sesión finalizada.");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return NoContentResponse("Si el email existe, recibirás instrucciones.");
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword), cancellationToken);
        return NoContentResponse("Contraseña actualizada correctamente.");
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new VerifyEmailCommand(request.Token), cancellationToken);
        return NoContentResponse("Email verificado.");
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUser.UserId ?? throw new UnauthorizedException();
        await Mediator.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);
        return NoContentResponse("Contraseña actualizada.");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = CurrentUser.UserId ?? throw new UnauthorizedException();
        var profile = await Mediator.Send(new GetProfileQuery(userId), cancellationToken);
        return Ok(profile);
    }
}

public sealed record VerifyEmailRequest(string Token);
