namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Handles a specific <see cref="IEvent"/>.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    #region Methods (Public)
    /// <summary>
    /// Handles the specified event instance.
    /// </summary>
    /// <param name="event">Event to handle.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous handling.</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
