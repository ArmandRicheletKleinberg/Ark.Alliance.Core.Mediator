namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Pre-processor executed before a query handler.
/// </summary>
/// <typeparam name="TQuery">Query type.</typeparam>
public interface IQueryPreProcessor<in TQuery> where TQuery : notnull
{
    /// <summary>
    /// Executes before the query handler.
    /// </summary>
    Task Process(TQuery query, CancellationToken cancellationToken);
}
