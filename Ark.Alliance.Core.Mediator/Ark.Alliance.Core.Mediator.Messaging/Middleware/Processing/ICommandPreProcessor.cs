namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Pre-processor executed before a command handler.
/// </summary>
/// <typeparam name="TCommand">Command type.</typeparam>
public interface ICommandPreProcessor<in TCommand> where TCommand : notnull
{
    /// <summary>
    /// Executes before the command handler.
    /// </summary>
    Task Process(TCommand command, CancellationToken cancellationToken);
}
