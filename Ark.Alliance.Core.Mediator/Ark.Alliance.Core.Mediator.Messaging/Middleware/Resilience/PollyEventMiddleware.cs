using Polly;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware applying a Polly policy when publishing events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class PollyEventMiddleware<TEvent> : IEventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly IAsyncPolicy _policy;

    public PollyEventMiddleware(IAsyncPolicy policy) => _policy = policy;

    /// <inheritdoc />
    public Task HandleAsync(
        TEvent @event,
        EventHandlerDelegate next,
        CancellationToken cancellationToken = default) =>
        _policy.ExecuteAsync(_ => next(), cancellationToken);
}
