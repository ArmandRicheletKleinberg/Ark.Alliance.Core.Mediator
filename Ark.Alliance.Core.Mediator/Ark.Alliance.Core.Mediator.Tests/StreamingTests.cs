using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using Ark.Alliance.Core.Mediator.Messaging.Streaming;
using System.Diagnostics;
using System.Threading.Channels;
using Xunit;

/// <summary>
/// Integration tests for streaming publishers and subscribers.
/// </summary>
public class StreamingTests
{
    /// <summary>
    /// Publishes and consumes integers using in-memory channels.
    /// </summary>
    [Fact]
    public async Task InMemory_stream_roundtrip()
    {
        var channel = Channel.CreateUnbounded<int>();
        var publisher = new InMemoryStreamPublisher<int>(channel);
        var subscriber = new InMemoryStreamSubscriber<int>(channel);

        var sw = Stopwatch.StartNew();

        var items = ToAsync(Enumerable.Range(1, 5));
        await foreach (var ack in publisher.PublishAsync(items, CancellationToken.None))
            Assert.Equal(Ack.Ok, ack);

        channel.Writer.Complete();

        var results = new List<int>();
        await foreach (var item in subscriber.ConsumeAsync(string.Empty, CancellationToken.None))
            results.Add(item);

        sw.Stop();

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, results);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(1), $"Streaming took too long: {sw.Elapsed}");
    }

    /// <summary>
    /// Writes stream data to disk and reads it back for verification.
    /// </summary>
    [Fact]
    public async Task File_stream_roundtrip()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var topic = "numbers";
        var publisher = new FileStreamPublisher<int>(dir, topic);
        var subscriber = new FileStreamSubscriber<int>(dir);

        var sw = Stopwatch.StartNew();

        var items = ToAsync(Enumerable.Range(1, 3));
        await foreach (var ack in publisher.PublishAsync(items, CancellationToken.None))
            Assert.Equal(Ack.Ok, ack);

        var results = new List<int>();
        await foreach (var item in subscriber.ConsumeAsync(topic, CancellationToken.None))
            results.Add(item);

        sw.Stop();

        Assert.Equal(new[] { 1, 2, 3 }, results);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(3), $"File streaming took too long: {sw.Elapsed}");
        Directory.Delete(dir, true);
    }

    private static async IAsyncEnumerable<int> ToAsync(IEnumerable<int> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
