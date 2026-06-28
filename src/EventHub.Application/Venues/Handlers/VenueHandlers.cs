using EventHub.Application.Common.Interfaces;
using EventHub.Application.Venues.DTOs;
using EventHub.Application.Venues.Queries;
using EventHub.Domain.Entities.Venues;
using EventHub.Shared.Exceptions;
using MediatR;

namespace EventHub.Application.Venues.Handlers;

public sealed class ListVenuesHandler : IRequestHandler<ListVenuesQuery, IReadOnlyList<VenueResponse>>
{
    private readonly IVenueRepository _repo;

    public ListVenuesHandler(IVenueRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<VenueResponse>> Handle(ListVenuesQuery request, CancellationToken cancellationToken)
    {
        var venues = await _repo.GetAllAsync(cancellationToken);
        return venues
            .Select(v => new VenueResponse(
                v.Id, v.Name, v.Address, v.City, v.State, v.Country,
                v.Latitude, v.Longitude, v.Capacity))
            .ToList();
    }
}

public sealed class GetVenueByIdHandler : IRequestHandler<GetVenueByIdQuery, VenueResponse?>
{
    private readonly IRepository<Venue> _repo;

    public GetVenueByIdHandler(IRepository<Venue> repo)
    {
        _repo = repo;
    }

    public async Task<VenueResponse?> Handle(GetVenueByIdQuery request, CancellationToken cancellationToken)
    {
        var v = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (v is null) return null;
        return new VenueResponse(
            v.Id, v.Name, v.Address, v.City, v.State, v.Country,
            v.Latitude, v.Longitude, v.Capacity);
    }
}
