namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Extension helpers to convert dispatcher results into <see cref="ResultDto"/> models.
/// These methods are optional and can be omitted on minimal platforms.
/// </summary>
public static class ArkDispatcherDtoExtensions
{
    #region Methods (Public)

    /// <summary>
    /// Sends a command and converts the resulting <see cref="Result{TResult}"/> into a <see cref="ResultDto{TResult}"/>.
    /// </summary>
    /// <typeparam name="TCommand">Command type to dispatch.</typeparam>
    /// <typeparam name="TResult">Type returned by the command handler.</typeparam>
    /// <param name="dispatcher">Dispatcher used to send the command.</param>
    /// <param name="command">Command instance to process.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A data transfer object representing the command result.</returns>
    /// <example>
    /// <code>
    /// var dto = await dispatcher.SendDtoAsync(new CreateOrder(), ct);
    /// </code>
    /// </example>
    public static async Task<ResultDto<TResult>> SendDtoAsync<TCommand, TResult>(this IArkDispatcher dispatcher, TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var result = await dispatcher.SendAsync<TCommand, TResult>(command, cancellationToken);
        return CreateDto(result);
    }

    /// <summary>
    /// Executes a query and converts the resulting <see cref="Result{TResult}"/> into a <see cref="ResultDto{TResult}"/>.
    /// </summary>
    /// <typeparam name="TQuery">Query type to dispatch.</typeparam>
    /// <typeparam name="TResult">Type returned by the query handler.</typeparam>
    /// <param name="dispatcher">Dispatcher used to execute the query.</param>
    /// <param name="query">Query instance to process.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A data transfer object representing the query result.</returns>
    /// <example>
    /// <code>
    /// var dto = await dispatcher.QueryDtoAsync(new GetOrders(), ct);
    /// </code>
    /// </example>
    public static async Task<ResultDto<TResult>> QueryDtoAsync<TQuery, TResult>(this IArkDispatcher dispatcher, TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        var result = await dispatcher.QueryAsync<TQuery, TResult>(query, cancellationToken);
        return CreateDto(result);
    }

    #endregion Methods (Public)

    #region Methods (Private)

    /// <summary>
    /// Converts a <see cref="Result{TResult}"/> into its serializable <see cref="ResultDto{TResult}"/> representation.
    /// </summary>
    /// <typeparam name="TResult">Type carried by the result.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>The DTO representation of the provided result.</returns>
    private static ResultDto<TResult> CreateDto<TResult>(Result<TResult> result) => new()
    {
        Data = result.Data,
        Status = result.Status,
        Reason = result.Reason,
        Exception = MapException(result.Exception)
    };

    /// <summary>
    /// Recursively converts an <see cref="Exception"/> into its DTO form.
    /// </summary>
    /// <param name="exception">Exception to map.</param>
    /// <returns>An <see cref="ExceptionDto"/> or <c>null</c> if <paramref name="exception"/> is <c>null</c>.</returns>
    private static ExceptionDto MapException(Exception exception) => exception == null
        ? null
        : new ExceptionDto
        {
            ExceptionType = exception.GetType().FullName,
            Data = exception.Data,
            HelpLink = exception.HelpLink,
            InnerException = MapException(exception.InnerException),
            Message = exception.Message,
            Source = exception.Source,
            StackTrace = exception.StackTrace
        };

    #endregion Methods (Private)
}
