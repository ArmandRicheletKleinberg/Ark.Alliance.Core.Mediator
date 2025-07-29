namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Options controlling retry behavior for <see cref="RetryCommandMiddleware{TCommand, TResult}"/>.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>
    /// Number of times to retry a failed command. Defaults to 3.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
