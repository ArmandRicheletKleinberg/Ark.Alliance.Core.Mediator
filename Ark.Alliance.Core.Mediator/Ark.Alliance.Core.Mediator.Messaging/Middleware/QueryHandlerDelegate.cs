namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading.Tasks;

/// <summary>
/// Delegate used to invoke the next element in a query handling pipeline.
/// </summary>
/// <typeparam name="TResult">Type returned by the query handler.</typeparam>
/// <returns>A task producing the handler result.</returns>
public delegate Task<Result<TResult>> QueryHandlerDelegate<TResult>();
