using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware that logs command execution using <see cref="ILogger"/>.
/// This allows integration with Ark.App.Diagnostics or any compatible
/// logging provider.
/// </summary>
/// <typeparam name="TCommand">Command type being processed.</typeparam>
/// <typeparam name="TResult">Result produced by the command.</typeparam>
public class LoggingCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ILogger<LoggingCommandMiddleware<TCommand, TResult>> _logger;

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="logger">Logger used to write diagnostic messages.</param>
    public LoggingCommandMiddleware(ILogger<LoggingCommandMiddleware<TCommand, TResult>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing command {Command}", typeof(TCommand).Name);
        try
        {
            var result = await next();
            if (result.Status == ResultStatus.Success)
                _logger.LogInformation("Command {Command} succeeded", typeof(TCommand).Name);
            else
                _logger.LogWarning("Command {Command} finished with status {Status} : {Reason}", typeof(TCommand).Name, result.Status, result.Reason);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {Command} threw an exception", typeof(TCommand).Name);
            throw;
        }
    }
}
