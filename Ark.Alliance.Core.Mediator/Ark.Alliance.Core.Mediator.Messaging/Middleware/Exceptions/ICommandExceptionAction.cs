namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Executes side effects when a command handler throws an exception.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TException">Exception type.</typeparam>
public interface ICommandExceptionAction<in TCommand, in TException>
    where TException : Exception
{
    /// <summary>
    /// Called when the command handler throws an exception.
    /// </summary>
    /// <param name="command">Command instance.</param>
    /// <param name="exception">The thrown exception.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(TCommand command, TException exception, CancellationToken cancellationToken);
}
