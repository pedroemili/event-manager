using System.Security.Claims;
using EventHub.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace EventHub.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles
    {
        get
        {
            if (Principal is null) return Array.Empty<string>();
            return Principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
    }

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
