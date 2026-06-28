using MediatR;

namespace EventHub.Application.Dashboard.Queries;

public sealed record GetOrganizerMetricsQuery : IRequest<DashboardMetricsResponse>;

public sealed record DashboardMetricsResponse(
    int ActiveEvents,
    int CompletedEvents,
    int TotalTicketsSold,
    decimal TotalRevenue,
    decimal MonthlyRevenue,
    double AttendanceRate,
    TopSellingEventInfo? TopSellingEvent
);

public sealed record TopSellingEventInfo(
    Guid Id,
    string Title,
    int SoldCount,
    decimal Revenue
);