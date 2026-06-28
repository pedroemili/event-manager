using EventHub.Application.Dashboard.Queries;
using MediatR;

namespace EventHub.Application.Dashboard.Handlers;

public sealed class GetOrganizerMetricsHandler : IRequestHandler<GetOrganizerMetricsQuery, DashboardMetricsResponse>
{
    private readonly Common.Interfaces.IEventRepository _eventRepo;

    public GetOrganizerMetricsHandler(Common.Interfaces.IEventRepository eventRepo)
    {
        _eventRepo = eventRepo;
    }

    public async Task<DashboardMetricsResponse> Handle(GetOrganizerMetricsQuery request, CancellationToken cancellationToken)
    {
        var allEvents = await _eventRepo.GetAllAsync(cancellationToken);

        var activeEvents = allEvents.Count(e => e.Status == "Published");
        var completedEvents = allEvents.Count(e => e.Status == "Completed");
        var ticketsSold = allEvents.Sum(e => e.TicketTypes.Sum(tt => tt.SoldQuantity));
        var totalRevenue = allEvents.Sum(e => e.TicketTypes.Sum(tt => tt.SoldQuantity * tt.Price));
        var monthlyRevenue = totalRevenue; // Simplified
        var attendanceRate = ticketsSold > 0
            ? (double)allEvents.Sum(e => e.Tickets.Count(t => t.Status == "Used")) / ticketsSold * 100
            : 0;

        var topEvent = allEvents
            .Where(e => e.TicketTypes.Sum(tt => tt.SoldQuantity) > 0)
            .Select(e => new TopSellingEventInfo(
                e.Id, e.Title,
                e.TicketTypes.Sum(tt => tt.SoldQuantity),
                e.TicketTypes.Sum(tt => tt.SoldQuantity * tt.Price)
            ))
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefault();

        return new DashboardMetricsResponse(
            activeEvents, completedEvents, ticketsSold, totalRevenue,
            monthlyRevenue, Math.Round(attendanceRate, 1), topEvent
        );
    }
}