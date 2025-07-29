namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Executes side effects when a query handler throws an exception.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TException">Exception type.</typeparam>
public interface IQueryExceptionAction<in TQuery, in TException>
    where TException : Exception
{
    /// <summary>
    /// Called when the query handler throws an exception.
    /// </summary>
    /// <param name="query">Query instance.</param>
    /// <param name="exception">The thrown exception.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(TQuery query, TException exception, CancellationToken cancellationToken);
}
