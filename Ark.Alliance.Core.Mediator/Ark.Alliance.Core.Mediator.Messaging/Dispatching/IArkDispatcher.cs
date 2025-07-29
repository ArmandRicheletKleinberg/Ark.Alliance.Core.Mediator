namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Central dispatcher used to invoke command, query and event handlers.
/// </summary>
public interface IArkDispatcher
{
    #region Methods (Public)
    /// <summary>
    /// Sends a command through the pipeline and returns its resulting value.
    /// </summary>
    /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
    /// <typeparam name="TResult">Type returned by the command handler.</typeparam>
    /// <param name="command">The command instance to process.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{TResult}"/> containing the command outcome.</returns>
    Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Executes a query through the pipeline and returns its resulting value.
    /// </summary>
    /// <typeparam name="TQuery">Type of query to dispatch.</typeparam>
    /// <typeparam name="TResult">Type returned by the query handler.</typeparam>
    /// <param name="query">The query instance to process.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{TResult}"/> containing the query outcome.</returns>
    Task<Result<TResult>> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;

    /// <summary>
    /// Sends a command using runtime dispatch. The result type is returned as <c>object</c>.
    /// </summary>
    /// <param name="command">Command instance implementing <see cref="ICommand{TResult}"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<Result<object?>> SendAsync(object command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a query using runtime dispatch. The result type is returned as <c>object</c>.
    /// </summary>
    /// <param name="query">Query instance implementing <see cref="IQuery{TResult}"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<Result<object?>> QueryAsync(object query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event instance using runtime dispatch.
    /// </summary>
    /// <param name="event">Event instance implementing <see cref="IEvent"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task PublishAsync(object @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an asynchronous stream using runtime dispatch.
    /// </summary>
    /// <param name="query">Streaming request implementing <see cref="IStreamRequest{T}"/>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    IAsyncEnumerable<object?> CreateStream(object query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to publish.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    /// <summary>
    /// Creates an asynchronous stream from a stream request.
    /// </summary>
    /// <typeparam name="T">Type of items produced in the stream.</typeparam>
    /// <param name="query">The streaming request to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>An asynchronous enumeration of <typeparamref name="T"/> items.</returns>
    IAsyncEnumerable<T> CreateStream<T>(IStreamRequest<T> query, CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
