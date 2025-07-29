namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware executing <see cref="ICommandPostProcessor{TCommand,TResult}"/> instances after the command handler.
/// </summary>
public class CommandPostProcessorMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IEnumerable<ICommandPostProcessor<TCommand, TResult>> _processors;

    #region Constructors

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="processors">Processors executed after the command handler.</param>
    public CommandPostProcessorMiddleware(IEnumerable<ICommandPostProcessor<TCommand, TResult>> processors)
    {
        _processors = processors ?? Enumerable.Empty<ICommandPostProcessor<TCommand, TResult>>();
    }

    #endregion Constructors

    #region Methods (Public)

    /// <summary>
    /// Invokes the next delegate then executes all post processors.
    /// </summary>
    /// <param name="command">Command being processed.</param>
    /// <param name="next">Delegate to invoke the actual handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The command result after postprocessing.</returns>
    public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var result = await next().ConfigureAwait(false);

        foreach (var processor in _processors)
            await processor.Process(command, result, cancellationToken).ConfigureAwait(false);

        return result;
    }

    #endregion Methods (Public)
}
