using AutoMapper;
using EventHub.Application.Auth.DTOs;
using EventHub.Domain.Entities.Users;

namespace EventHub.Application.Common.Mappings;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserProfileResponse>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => ExtractRoleNames(s.UserRoles)));
    }

    private static IReadOnlyList<string> ExtractRoleNames(ICollection<UserRole> userRoles)
    {
        if (userRoles == null) return Array.Empty<string>();
        return userRoles
            .Select(ur => ur.Role == null ? "" : ur.Role.Name)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();
    }
}
