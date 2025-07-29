namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading.Tasks;

/// <summary>
/// Delegate used to invoke the next element in an event handling pipeline.
/// </summary>
/// <returns>A task that completes when the next handler has finished processing.</returns>
public delegate Task EventHandlerDelegate();
