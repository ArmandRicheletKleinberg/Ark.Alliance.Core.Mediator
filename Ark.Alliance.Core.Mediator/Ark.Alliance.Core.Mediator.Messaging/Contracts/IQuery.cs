namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Represents a read-only request that returns a <typeparamref name="TResult"/> value.
/// </summary>
/// <typeparam name="TResult">Type returned by the query handler.</typeparam>
/// <example>
/// <code>
/// var customers = await dispatcher.QueryAsync(new GetCustomers(), ct);
/// </code>
/// </example>
public interface IQuery<TResult> { }
