namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading.Tasks;

/// <summary>
/// Delegate used to invoke the next element in a command handling pipeline.
/// </summary>
/// <typeparam name="TResult">Type returned by the command handler.</typeparam>
/// <returns>A task producing the handler result.</returns>
public delegate Task<Result<TResult>> CommandHandlerDelegate<TResult>();
