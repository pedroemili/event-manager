using EventHub.Application.Common.Interfaces;
using EventHub.Application.Events.DTOs;
using EventHub.Shared.Responses;
using MediatR;

namespace EventHub.Application.Events.Queries;

public sealed record GetEventBySlugQuery(string Slug) : IRequest<EventDetailResponse?>;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<EventDetailResponse?>;

public sealed record GetMyEventsQuery : IRequest<IReadOnlyList<EventListResponse>>;

public sealed record GetPublishedEventsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    DateTime? FromDate = null,
    bool? Featured = null,
    bool Upcoming = true
) : IRequest<EventHub.Shared.Responses.PagedResult<EventListResponse>>;