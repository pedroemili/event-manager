using EventHub.Application.Common.Interfaces;
using EventHub.Application.Events.DTOs;
using EventHub.Domain.Entities.Events;
using EventHub.Shared.Exceptions;
using EventHub.Shared.Helpers;
using MediatR;

namespace EventHub.Application.Events.Commands;

public sealed record CreateEventCommand(
    string Title,
    string? ShortDescription,
    string? Description,
    Guid? CategoryId,
    Guid? VenueId,
    DateTime StartDate,
    DateTime EndDate,
    string? Timezone,
    int? MaxAttendees,
    int? AgeRestriction,
    string Visibility
) : IRequest<EventDetailResponse>;

public sealed record UpdateEventCommand(
    Guid Id,
    string Title,
    string? ShortDescription,
    string? Description,
    Guid? CategoryId,
    Guid? VenueId,
    DateTime StartDate,
    DateTime EndDate,
    string? Timezone,
    int? MaxAttendees,
    int? AgeRestriction,
    string Visibility
) : IRequest<EventDetailResponse>;

public sealed record PublishEventCommand(Guid Id) : IRequest<EventDetailResponse>;

public sealed record CancelEventCommand(Guid Id, string Reason) : IRequest<EventDetailResponse>;

public sealed record DeleteEventCommand(Guid Id) : IRequest;

public sealed record ToggleFavoriteCommand(Guid EventId) : IRequest<bool>;