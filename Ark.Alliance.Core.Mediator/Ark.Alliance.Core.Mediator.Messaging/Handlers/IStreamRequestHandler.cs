namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Handles a streaming request by producing an asynchronous sequence of results.
/// </summary>
/// <typeparam name="TReq">Type of request handled.</typeparam>
/// <typeparam name="T">Type of items produced.</typeparam>
public interface IStreamRequestHandler<in TReq, T>
    where TReq : IStreamRequest<T>
{
    #region Methods (Public)
    /// <summary>
    /// Handles the specified request and returns a stream of items.
    /// </summary>
    /// <param name="req">Request to handle.</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    /// <returns>An asynchronous stream of <typeparamref name="T"/> values.</returns>
    IAsyncEnumerable<T> HandleAsync(TReq req, CancellationToken ct);

    #endregion Methods (Public)
}
