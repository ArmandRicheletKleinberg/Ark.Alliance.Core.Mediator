using Polly;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware applying a Polly <see cref="IAsyncPolicy{TResult}"/> when executing commands.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public class PollyCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IAsyncPolicy _policy;

    /// <summary>
    /// Creates a new middleware instance using the provided policy.
    /// </summary>
    /// <param name="policy">Policy used to execute the handler.</param>
    public PollyCommandMiddleware(IAsyncPolicy policy) => _policy = policy;

    /// <inheritdoc />
    public Task<Result<TResult>> HandleAsync(
        TCommand command,
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default) =>
        _policy.ExecuteAsync(_ => next(), cancellationToken);
}
