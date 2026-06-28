using EventHub.Application.Venues.DTOs;
using MediatR;

namespace EventHub.Application.Venues.Queries;

public sealed record ListVenuesQuery : IRequest<IReadOnlyList<VenueResponse>>;

public sealed record GetVenueByIdQuery(Guid Id) : IRequest<VenueResponse?>;
