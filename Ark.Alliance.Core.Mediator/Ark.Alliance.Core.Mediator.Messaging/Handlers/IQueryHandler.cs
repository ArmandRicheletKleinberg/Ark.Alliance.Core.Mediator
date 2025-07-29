namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Handles a specific <see cref="IQuery{TResult}"/>.
/// </summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    #region Methods (Public)
    /// <summary>
    /// Handles the provided query instance.
    /// </summary>
    /// <param name="query">Query to handle.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{TResult}"/> produced by the query logic.</returns>
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
