namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Handles a specific <see cref="ICommand{TResult}"/>.
/// </summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    #region Methods (Public)
    /// <summary>
    /// Handles the provided command instance.
    /// </summary>
    /// <param name="command">Command to handle.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result{TResult}"/> produced by the command logic.</returns>
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);

    #endregion Methods (Public)
}
