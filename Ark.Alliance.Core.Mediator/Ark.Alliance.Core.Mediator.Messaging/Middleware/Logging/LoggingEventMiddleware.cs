using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware that logs event publication using <see cref="ILogger"/>.
/// Compatible with Ark.App.Diagnostics or any standard logging provider.
/// </summary>
/// <typeparam name="TEvent">Event type being processed.</typeparam>
public class LoggingEventMiddleware<TEvent> : IEventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly ILogger<LoggingEventMiddleware<TEvent>> _logger;

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="logger">Logger used to write diagnostic messages.</param>
    public LoggingEventMiddleware(ILogger<LoggingEventMiddleware<TEvent>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        TEvent @event,
        EventHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing event {Event}", typeof(TEvent).Name);
        try
        {
            await next();
            _logger.LogInformation("Event {Event} handled", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event {Event} threw an exception", typeof(TEvent).Name);
            throw;
        }
    }
}
