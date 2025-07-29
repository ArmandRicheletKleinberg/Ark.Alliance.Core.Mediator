namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware that leverages <see cref="ICommandDecisionService"/>
/// to analyse a command and optionally short circuit execution
/// or augment the result with AI insights.
/// </summary>
/// <typeparam name="TCommand">Command type being processed.</typeparam>
/// <typeparam name="TResult">Result produced by the command.</typeparam>
public class AiCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ICommandDecisionService _decisionService;

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="decisionService">Service used to analyse commands.</param>
    public AiCommandMiddleware(ICommandDecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    /// <inheritdoc />
    public async Task<Result<TResult>> HandleAsync(
        TCommand command,
        CommandHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var decision = await _decisionService.EvaluateAsync<TCommand, TResult>(command, cancellationToken);
        if (decision.Status != ResultStatus.Success)
        {
            // short circuit: return decision converted to Result<TResult>
            return FromBaseResult(decision);
        }

        var result = await next();
        return result;
    }

    private static Result<TResult> FromBaseResult(Result decision)
    {
        var converted = new Result<TResult>(decision.Status, default, decision.Exception)
            .WithReason(decision.Reason);
        return converted;
    }
}
