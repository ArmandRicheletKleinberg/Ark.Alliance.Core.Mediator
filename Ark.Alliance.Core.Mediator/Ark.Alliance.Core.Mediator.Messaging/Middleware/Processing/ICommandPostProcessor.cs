namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Post-processor executed after a command handler.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface ICommandPostProcessor<in TCommand, TResult> where TCommand : notnull
{
    /// <summary>
    /// Executes after the command handler.
    /// </summary>
    Task Process(TCommand command, Result<TResult> response, CancellationToken cancellationToken);
}
