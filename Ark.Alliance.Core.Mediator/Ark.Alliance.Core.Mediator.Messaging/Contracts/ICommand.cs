namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Represents a write operation that produces a <typeparamref name="TResult"/> value.
/// </summary>
/// <typeparam name="TResult">Type returned after the command is handled.</typeparam>
/// <example>
/// <code>
/// var result = await dispatcher.SendAsync(new CreateOrder(), ct);
/// </code>
/// </example>
public interface ICommand<TResult> { }
