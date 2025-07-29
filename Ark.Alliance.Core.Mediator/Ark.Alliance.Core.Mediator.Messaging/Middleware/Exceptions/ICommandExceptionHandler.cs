namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles exceptions thrown by a command handler.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResult">Command result type.</typeparam>
/// <typeparam name="TException">Exception type.</typeparam>
public interface ICommandExceptionHandler<in TCommand, TResult, in TException>
    where TCommand : ICommand<TResult>
    where TException : Exception
{
    /// <summary>
    /// Called when a command handler throws an exception.
    /// </summary>
    /// <param name="command">Command instance.</param>
    /// <param name="exception">The thrown exception.</param>
    /// <param name="state">Exception handling state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(TCommand command, TException exception, CommandExceptionHandlerState<TResult> state, CancellationToken cancellationToken);
}
