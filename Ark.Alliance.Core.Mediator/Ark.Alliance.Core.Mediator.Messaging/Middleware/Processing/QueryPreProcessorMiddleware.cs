namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware executing <see cref="IQueryPreProcessor{TQuery}"/> instances before the query handler.
/// </summary>
public class QueryPreProcessorMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IEnumerable<IQueryPreProcessor<TQuery>> _processors;

    #region Constructors

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="processors">Processors executed before the query handler.</param>
    public QueryPreProcessorMiddleware(IEnumerable<IQueryPreProcessor<TQuery>> processors)
    {
        _processors = processors ?? Enumerable.Empty<IQueryPreProcessor<TQuery>>();
    }

    #endregion Constructors

    #region Methods (Public)

    /// <summary>
    /// Executes all pre processors then invokes the next delegate.
    /// </summary>
    /// <param name="query">Query being processed.</param>
    /// <param name="next">Delegate to invoke the actual handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The query result after preprocessing.</returns>
    public async Task<Result<TResult>> HandleAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        foreach (var processor in _processors)
            await processor.Process(query, cancellationToken).ConfigureAwait(false);

        return await next().ConfigureAwait(false);
    }

    #endregion Methods (Public)
}
