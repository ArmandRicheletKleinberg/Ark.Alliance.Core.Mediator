namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Middleware executed around stream request handlers.
/// </summary>
/// <typeparam name="TRequest">Request type.</typeparam>
/// <typeparam name="T">Item type returned.</typeparam>
public interface IStreamMiddleware<TRequest, T>
    where TRequest : IStreamRequest<T>
{
    /// <summary>
    /// Invokes the middleware logic for the specified request.
    /// </summary>
    /// <param name="request">Request being processed.</param>
    /// <param name="next">Delegate to invoke the next middleware or handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>An asynchronous stream of <typeparamref name="T"/>.</returns>
    IAsyncEnumerable<T> HandleAsync(
        TRequest request,
        StreamHandlerDelegate<T> next,
        CancellationToken cancellationToken = default);
}
