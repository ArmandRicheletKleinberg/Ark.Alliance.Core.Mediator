using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;

namespace Ark.Alliance.Core.Mediator.Messaging.Streaming;

/// <summary>
/// Stream subscriber that reads items from newline separated JSON files.
/// </summary>
/// <typeparam name="T">Type of items consumed.</typeparam>
public class FileStreamSubscriber<T>(string basePath) : IStreamSubscriber<T>
{
    private readonly string _basePath = basePath;

    #region Public Methods
    /// <inheritdoc />
    public async IAsyncEnumerable<T> ConsumeAsync(string topic, [EnumeratorCancellation] CancellationToken ct)
    {
        var path = Path.Combine(_basePath, $"{topic}.jsonl");
        if (!File.Exists(path)) yield break;

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        string? line;
        while (!ct.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null)
        {
            var item = JsonSerializer.Deserialize<T>(line!);
            if (item is not null)
                yield return item;
        }
    }
    #endregion Public Methods
}
