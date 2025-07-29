namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executed around event handlers.
/// </summary>
public interface IEventMiddleware<TEvent> where TEvent : IEvent
{
    #region Methods (Public)
    /// <summary>
    /// Invokes the middleware logic for the specified event.
    /// </summary>
    /// <param name="event">Event being processed.</param>
    /// <param name="next">Delegate to invoke the next middleware or handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(
        TEvent @event,
        EventHandlerDelegate next,
        CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
