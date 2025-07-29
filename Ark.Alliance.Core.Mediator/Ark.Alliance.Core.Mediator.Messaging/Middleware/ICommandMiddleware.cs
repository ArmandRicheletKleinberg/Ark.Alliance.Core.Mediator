using System.Threading;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware executed around command handlers.
/// </summary>
public interface ICommandMiddleware<TCommand, TResult> where TCommand : ICommand<TResult>
{
    #region Methods (Public)
    /// <summary>
    /// Invokes the middleware logic for the specified command.
    /// </summary>
    /// <param name="command">Command being processed.</param>
    /// <param name="next">Delegate to invoke the next middleware or handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The result returned by the command handler chain.</returns>
    Task<Result<TResult>> HandleAsync(
        TCommand command,
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
