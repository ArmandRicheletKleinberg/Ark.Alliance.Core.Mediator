namespace Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;

/// <summary>
/// Simple diagnostic report for a queue.
/// </summary>
/// <param name="Queue">Queue name.</param>
/// <param name="MessageCount">Current number of messages.</param>
/// <param name="SampleMessages">Optional first messages for inspection.</param>
public sealed record QueueReport(string Queue, uint MessageCount, List<string> SampleMessages);

