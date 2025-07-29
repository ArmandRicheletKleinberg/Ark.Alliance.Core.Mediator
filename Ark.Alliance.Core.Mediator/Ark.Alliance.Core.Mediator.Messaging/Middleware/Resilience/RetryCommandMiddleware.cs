using Polly;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware that retries command execution using <see cref="Polly"/> policies.
/// </summary>
/// <typeparam name="TCommand">Command type being processed.</typeparam>
/// <typeparam name="TResult">Result produced by the command.</typeparam>
public sealed class RetryCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly AsyncPolicy<Result<TResult>> _policy;

    /// <summary>
    /// Creates a new instance using provided <see cref="RetryOptions"/>.
    /// </summary>
    /// <param name="options">Retry configuration.</param>
    public RetryCommandMiddleware(RetryOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (options.RetryCount < 0)
            throw new ArgumentOutOfRangeException(nameof(options.RetryCount));

        // Polly interprets the retry count parameter as the total number of
        // executions to perform. The middleware, however, exposes
        // <see cref="RetryOptions.RetryCount"/> as the number of retry attempts
        // after the initial execution. Add one to align with this expectation.
        _policy = Policy<Result<TResult>>
            .Handle<Exception>()
            .RetryAsync(options.RetryCount + 1);
    }

    /// <inheritdoc />
    public Task<Result<TResult>> HandleAsync(
        TCommand command,
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        return _policy.ExecuteAsync(_ => next(), cancellationToken);
    }
}
