using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Middleware that logs query execution using <see cref="ILogger"/>.
/// Compatible with Ark.App.Diagnostics or any standard logging provider.
/// </summary>
/// <typeparam name="TQuery">Query type being processed.</typeparam>
/// <typeparam name="TResult">Result produced by the query.</typeparam>
public class LoggingQueryMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly ILogger<LoggingQueryMiddleware<TQuery, TResult>> _logger;

    /// <summary>
    /// Creates a new instance of the middleware.
    /// </summary>
    /// <param name="logger">Logger used to write diagnostic messages.</param>
    public LoggingQueryMiddleware(ILogger<LoggingQueryMiddleware<TQuery, TResult>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        QueryHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing query {Query}", typeof(TQuery).Name);
        try
        {
            var result = await next();
            if (result.Status == ResultStatus.Success)
                _logger.LogInformation("Query {Query} succeeded", typeof(TQuery).Name);
            else
                _logger.LogWarning("Query {Query} finished with status {Status} : {Reason}", typeof(TQuery).Name, result.Status, result.Reason);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query {Query} threw an exception", typeof(TQuery).Name);
            throw;
        }
    }
}
