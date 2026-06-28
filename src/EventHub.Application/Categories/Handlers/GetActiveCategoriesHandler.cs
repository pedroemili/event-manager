using EventHub.Application.Categories.DTOs;
using EventHub.Application.Categories.Queries;
using EventHub.Application.Common.Interfaces;
using MediatR;

namespace EventHub.Application.Categories.Handlers;

public sealed class GetActiveCategoriesHandler : IRequestHandler<GetActiveCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _repo;

    public GetActiveCategoriesHandler(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<CategoryResponse>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repo.GetActiveAsync(cancellationToken);
        return categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Slug, c.Description, c.IconName))
            .ToList();
    }
}
