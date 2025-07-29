namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware executing <see cref="ICommandPreProcessor{TCommand}"/> instances before the command handler.
/// </summary>
public class CommandPreProcessorMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IEnumerable<ICommandPreProcessor<TCommand>> _processors;

    #region Constructors

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="processors">Processors executed before the command handler.</param>
    public CommandPreProcessorMiddleware(IEnumerable<ICommandPreProcessor<TCommand>> processors)
    {
        _processors = processors ?? Enumerable.Empty<ICommandPreProcessor<TCommand>>();
    }

    #endregion Constructors

    #region Methods (Public)

    /// <summary>
    /// Executes all pre processors then invokes the next delegate.
    /// </summary>
    /// <param name="command">Command being processed.</param>
    /// <param name="next">Delegate to invoke the actual handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The command result after preprocessing.</returns>
    public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        foreach (var processor in _processors)
            await processor.Process(command, cancellationToken).ConfigureAwait(false);

        return await next().ConfigureAwait(false);
    }

    #endregion Methods (Public)
}
