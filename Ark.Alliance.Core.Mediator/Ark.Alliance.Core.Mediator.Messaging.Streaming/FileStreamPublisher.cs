using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// Stream publisher that persists items to newline separated JSON files.
/// </summary>
/// <typeparam name="T">Type of items being published.</typeparam>
public class FileStreamPublisher<T>(string basePath, string topic) : IStreamPublisher<T>
{
    private readonly string _path = Path.Combine(basePath, $"{topic}.jsonl");

    #region Public Methods
    /// <inheritdoc />
    public async IAsyncEnumerable<Ack> PublishAsync(IAsyncEnumerable<T> source, [EnumeratorCancellation] CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await using var stream = new StreamWriter(new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        await foreach (var item in source.WithCancellation(ct))
        {
            var json = JsonSerializer.Serialize(item);
            await stream.WriteLineAsync(json);
            await stream.FlushAsync();
            yield return Ack.Ok;
        }
    }
    #endregion Public Methods
}
