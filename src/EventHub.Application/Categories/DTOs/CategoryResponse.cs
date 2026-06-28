namespace EventHub.Application.Categories.DTOs;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? IconName);
