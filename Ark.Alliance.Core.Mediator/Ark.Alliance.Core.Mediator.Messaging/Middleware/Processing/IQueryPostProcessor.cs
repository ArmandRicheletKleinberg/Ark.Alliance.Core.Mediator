namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Post-processor executed after a query handler.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IQueryPostProcessor<in TQuery, TResult> where TQuery : notnull
{
    /// <summary>
    /// Executes after the query handler.
    /// </summary>
    Task Process(TQuery query, Result<TResult> response, CancellationToken cancellationToken);
}
