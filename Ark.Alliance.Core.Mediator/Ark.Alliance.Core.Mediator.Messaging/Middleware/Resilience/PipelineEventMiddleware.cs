using Microsoft.Extensions.Resilience;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executing events through a <see cref="ResiliencePipeline"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class PipelineEventMiddleware<TEvent> : IEventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly ResiliencePipeline _pipeline;

    /// <summary>
    /// Creates the middleware with a pipeline provider.
    /// </summary>
    /// <param name="provider">Resilience pipeline provider.</param>
    public PipelineEventMiddleware(ResiliencePipelineProvider provider)
        => _pipeline = provider.GetPipeline("default");

    /// <inheritdoc />
    public Task HandleAsync(TEvent @event, EventHandlerDelegate next, CancellationToken cancellationToken = default)
        => _pipeline.ExecuteAsync(_ => next(), cancellationToken);
}
