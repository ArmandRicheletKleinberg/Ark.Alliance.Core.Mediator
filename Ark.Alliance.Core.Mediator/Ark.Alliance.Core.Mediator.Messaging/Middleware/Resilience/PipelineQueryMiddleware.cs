using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Resilience;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executing queries through a <see cref="ResiliencePipeline{TResult}"/>.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class PipelineQueryMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly ResiliencePipeline<TResult> _pipeline;

    /// <summary>
    /// Creates the middleware with a pipeline provider.
    /// </summary>
    /// <param name="provider">Resilience pipeline provider.</param>
    public PipelineQueryMiddleware(ResiliencePipelineProvider provider)
        => _pipeline = provider.GetPipeline<TResult>("default");

    /// <inheritdoc />
    public Task<Result<TResult>> HandleAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
        => _pipeline.ExecuteAsync(_ => next(), cancellationToken);
}
