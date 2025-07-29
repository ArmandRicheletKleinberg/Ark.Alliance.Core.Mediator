namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware executing <see cref="IQueryPostProcessor{TQuery,TResult}"/> instances after the query handler.
/// </summary>
public class QueryPostProcessorMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IEnumerable<IQueryPostProcessor<TQuery, TResult>> _processors;

    #region Constructors

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="processors">Processors executed after the query handler.</param>
    public QueryPostProcessorMiddleware(IEnumerable<IQueryPostProcessor<TQuery, TResult>> processors)
    {
        _processors = processors ?? Enumerable.Empty<IQueryPostProcessor<TQuery, TResult>>();
    }

    #endregion Constructors

    #region Methods (Public)

    /// <summary>
    /// Invokes the next delegate then executes all post processors.
    /// </summary>
    /// <param name="query">Query being processed.</param>
    /// <param name="next">Delegate to invoke the actual handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The query result after postprocessing.</returns>
    public async Task<Result<TResult>> HandleAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var result = await next().ConfigureAwait(false);

        foreach (var processor in _processors)
            await processor.Process(query, result, cancellationToken).ConfigureAwait(false);

        return result;
    }

    #endregion Methods (Public)
}
