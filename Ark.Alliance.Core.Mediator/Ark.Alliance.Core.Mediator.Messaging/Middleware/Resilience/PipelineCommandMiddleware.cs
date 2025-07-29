using Microsoft.Extensions.Resilience;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executing commands through a <see cref="ResiliencePipeline{TResult}"/>.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class PipelineCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ResiliencePipeline<TResult> _pipeline;

    /// <summary>
    /// Creates the middleware with a pipeline provider.
    /// </summary>
    /// <param name="provider">Resilience pipeline provider.</param>
    public PipelineCommandMiddleware(ResiliencePipelineProvider provider)
        => _pipeline = provider.GetPipeline<TResult>("default");

    /// <inheritdoc />
    public Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
        => _pipeline.ExecuteAsync(_ => next(), cancellationToken);
}
