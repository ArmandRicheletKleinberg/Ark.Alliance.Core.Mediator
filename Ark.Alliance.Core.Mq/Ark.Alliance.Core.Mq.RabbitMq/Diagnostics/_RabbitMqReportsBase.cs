using Ark.Alliance.Core.Diagnostics;

namespace Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;

/// <summary>
/// Base class used to access diagnostics reports for RabbitMQ.
/// </summary>
public abstract class RabbitMqReportsBase : ReportsBase
{
    /// <summary>
    /// Get the messages from a queue.
    /// </summary>
    /// <param name="repository">The diagnostics repository.</param>
    /// <param name="queue">The queue name.</param>
    /// <param name="maxMessagesNumber">The maximum number of messages to return.</param>
    /// <returns>
    /// Success : The execution has succeeded and the data of raw string messages is returned.
    /// NotFound : No message was found in the queue.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    protected async Task<Result<List<string>>> GetReportQueueMessages(DiagnosticsRabbitMqRepository repository, string queue, int maxMessagesNumber)
    {
        var resultGetMessages = await repository.GetQueueXFirstMessages(queue, maxMessagesNumber);

        if (resultGetMessages.IsNotSuccess)
            return new Result<List<string>>(resultGetMessages);

        return new Result<List<string>>(resultGetMessages.Data);
    }

    /// <summary>
    /// Build a diagnostic report for a queue including length and sample messages.
    /// </summary>
    protected async Task<Result<QueueReport>> GetQueueReport(DiagnosticsRabbitMqRepository repository, string queue, int sampleCount)
    {
        var length = await repository.GetQueueLength(queue);
        if (length.IsNotSuccess)
            return new Result<QueueReport>(length);

        var msgs = await repository.GetQueueXFirstMessages(queue, sampleCount);
        if (msgs.IsNotSuccess && msgs.Status != ResultStatus.NotFound)
            return new Result<QueueReport>(msgs);

        var report = new QueueReport(queue, length.Data, msgs.IsSuccess ? msgs.Data : new List<string>());
        return new Result<QueueReport>(report);
    }
}
