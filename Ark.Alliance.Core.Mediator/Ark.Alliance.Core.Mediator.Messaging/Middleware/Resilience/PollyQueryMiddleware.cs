using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware applying a Polly <see cref="IAsyncPolicy{TResult}"/> when executing queries.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public class PollyQueryMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IAsyncPolicy _policy;

    public PollyQueryMiddleware(IAsyncPolicy policy) => _policy = policy;

    /// <inheritdoc />
    public Task<Result<TResult>> HandleAsync(
        TQuery query,
        QueryHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default) =>
        _policy.ExecuteAsync(_ => next(), cancellationToken);
}
