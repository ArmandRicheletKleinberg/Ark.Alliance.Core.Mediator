namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executed around query handlers.
/// </summary>
public interface IQueryMiddleware<TQuery, TResult> where TQuery : IQuery<TResult>
{
    #region Methods (Public)
    /// <summary>
    /// Invokes the middleware logic for the specified query.
    /// </summary>
    /// <param name="query">Query being processed.</param>
    /// <param name="next">Delegate to invoke the next middleware or handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The result returned by the query handler chain.</returns>
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        QueryHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
