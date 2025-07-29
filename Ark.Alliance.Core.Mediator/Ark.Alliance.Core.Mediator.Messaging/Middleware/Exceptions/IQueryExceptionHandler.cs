namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles exceptions thrown by a query handler.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TResult">Query result type.</typeparam>
/// <typeparam name="TException">Exception type.</typeparam>
public interface IQueryExceptionHandler<in TQuery, TResult, in TException>
    where TQuery : IQuery<TResult>
    where TException : Exception
{
    /// <summary>
    /// Called when a query handler throws an exception.
    /// </summary>
    /// <param name="query">Query instance.</param>
    /// <param name="exception">The thrown exception.</param>
    /// <param name="state">Exception handling state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TQuery query, TException exception, QueryExceptionHandlerState<TResult> state, CancellationToken cancellationToken);
}
