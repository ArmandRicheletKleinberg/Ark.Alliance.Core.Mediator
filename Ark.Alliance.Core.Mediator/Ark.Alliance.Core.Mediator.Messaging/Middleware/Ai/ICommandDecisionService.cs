namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Service used by AI middlewares to analyse commands or queries
/// and provide contextual decisions.
/// </summary>
public interface ICommandDecisionService
{
    /// <summary>
    /// Analyses the command before execution and optionally
    /// returns a <see cref="Result"/> indicating whether to
    /// proceed, modify the command or short circuit the pipeline.
    /// </summary>
    /// <typeparam name="TCommand">Command type.</typeparam>
    /// <typeparam name="TResult">Result type returned by the handler.</typeparam>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> describing the decision.</returns>
    Task<Result> EvaluateAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
