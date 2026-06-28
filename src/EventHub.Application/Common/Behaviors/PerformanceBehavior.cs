using System.Diagnostics;
using EventHub.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EventHub.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;
    private static readonly Activity Activity = new($"EventHub.Mediator.{typeof(TRequest).Name}");

    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer = new();

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Restart();
        var response = await next(cancellationToken);
        _timer.Stop();

        var elapsed = _timer.ElapsedMilliseconds;
        if (elapsed > SlowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("Slow request detected: {RequestName} took {Elapsed}ms", requestName, elapsed);
        }

        return response;
    }
}
