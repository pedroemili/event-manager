using EventHub.Application.Categories.DTOs;
using MediatR;

namespace EventHub.Application.Categories.Queries;

public sealed record GetActiveCategoriesQuery : IRequest<IReadOnlyList<CategoryResponse>>;
