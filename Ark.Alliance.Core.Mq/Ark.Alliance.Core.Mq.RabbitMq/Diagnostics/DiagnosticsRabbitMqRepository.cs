using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ark.Alliance.Core;
using Microsoft.Extensions.Logging;


namespace Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;

/// <summary>
/// Repository used to perform diagnostics operations on RabbitMQ.
/// </summary>
public class DiagnosticsRabbitMqRepository : RabbitMqRepositoryBase
{
    public DiagnosticsRabbitMqRepository(IChannelPool pool, ILogger<DiagnosticsRabbitMqRepository> logger)
        : base(pool, logger)
    {
    }

    /// <summary>
    /// Read the first messages of a queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="maxMessagesNumber">Number of messages to read.</param>
    /// <returns>
    /// Success : The execution has succeeded and the list of messages has been returned.
    /// NotFound : No message was found in the queue.
    /// Unexpected : An unexpected error occurs.
    /// </returns>
    public Task<Result<List<string>>> GetQueueXFirstMessages(string queue, int maxMessagesNumber)
        => Execute(async model =>
        {
            var messages = new List<string>();
            for (var i = 0; i < maxMessagesNumber; i++)
            {
                var result = await model.BasicGetAsync(queue, true);
                if (result == null)
                {
                    if (messages.Count == 0)
                        return Result<List<string>>.NotFound;
                    break;
                }
                messages.Add(Encoding.UTF8.GetString(result.Body.ToArray()));
            }
            return new Result<List<string>>(messages);
        });

    /// <summary>Gets the current length of a queue.</summary>
    public Task<Result<uint>> GetQueueLength(string queue) => GetQueueLengthInternal(queue);

    /// <summary>Purges all messages from a queue.</summary>
    public Task<Result<uint>> PurgeQueue(string queue) => PurgeQueueInternal(queue);
}
