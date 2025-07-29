namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Marker interface for a notification event published through the dispatcher.
/// </summary>
/// <remarks>
/// Events are fire-and-forget messages used to broadcast state changes across
/// the system.
/// </remarks>
/// <example>
/// <code>
/// await dispatcher.PublishAsync(new UserCreatedEvent(id), ct);
/// </code>
/// </example>
public interface IEvent { }
