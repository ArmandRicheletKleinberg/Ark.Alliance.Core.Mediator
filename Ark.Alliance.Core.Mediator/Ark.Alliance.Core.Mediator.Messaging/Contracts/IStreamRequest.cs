namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Marker interface representing a request that yields a stream of <typeparamref name="T"/> values.
/// </summary>
/// <typeparam name="T">Type of items produced by the stream.</typeparam>
/// <remarks>
/// Stream requests allow handlers to push multiple results asynchronously. They are commonly
/// consumed using the <c>await foreach</c> syntax.
/// </remarks>
/// <example>
/// <code>
/// await foreach(var item in dispatcher.CreateStream(new GetUpdates(), ct))
/// {
///     Console.WriteLine(item);
/// }
/// </code>
/// </example>
public interface IStreamRequest<out T> { }
