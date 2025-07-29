namespace Ark.Alliance.Core.Mediator.Messaging;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Delegate used to invoke the next element in a streaming pipeline.
/// </summary>
/// <typeparam name="T">Type of streamed items.</typeparam>
/// <returns>An async enumerable of <typeparamref name="T"/>.</returns>
public delegate IAsyncEnumerable<T> StreamHandlerDelegate<T>();
