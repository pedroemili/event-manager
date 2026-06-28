using System.Text.Json;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace EventHub.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException vex)
        {
            var payload = new
            {
                Success = false,
                Message = "Validation failed.",
                Code = "VALIDATION_ERROR",
                Errors = vex.Errors
            };
            await Write(context, StatusCodes.Status400BadRequest, payload);
        }
        catch (BadRequestException brex)
        {
            await Write(context, StatusCodes.Status400BadRequest, ApiResponse.Fail(brex.Message, "BAD_REQUEST"));
        }
        catch (UnauthorizedException uex)
        {
            await Write(context, StatusCodes.Status401Unauthorized, ApiResponse.Fail(uex.Message, "UNAUTHORIZED"));
        }
        catch (ForbiddenException fex)
        {
            await Write(context, StatusCodes.Status403Forbidden, ApiResponse.Fail(fex.Message, "FORBIDDEN"));
        }
        catch (NotFoundException nex)
        {
            await Write(context, StatusCodes.Status404NotFound, ApiResponse.Fail(nex.Message, "NOT_FOUND"));
        }
        catch (ConflictException cex)
        {
            await Write(context, StatusCodes.Status409Conflict, ApiResponse.Fail(cex.Message, "CONFLICT"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            await Write(context, StatusCodes.Status500InternalServerError, ApiResponse.Fail(
                "Ha ocurrido un error inesperado. Por favor intenta más tarde.",
                "INTERNAL_ERROR"));
        }
    }

    private static async Task Write(HttpContext context, int statusCode, object response)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions);
    }
}
