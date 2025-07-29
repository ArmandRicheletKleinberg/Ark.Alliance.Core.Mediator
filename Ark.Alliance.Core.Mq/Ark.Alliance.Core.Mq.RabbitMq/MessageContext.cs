using System;
using System.Collections.Generic;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Encapsulates a message payload along with optional headers
/// and correlation identifiers.
/// </summary>
/// <typeparam name="TPayload">Type of the message payload.</typeparam>
/// <example>
/// <code>
/// var ctx = new MessageContext<MyPayload>(new MyPayload());
/// </code>
/// </example>
public sealed record MessageContext<TPayload> where TPayload : class
{
    /// <summary>
    /// Creates a new <see cref="MessageContext{TPayload}"/> instance.
    /// </summary>
    /// <param name="payload">Message payload.</param>
    /// <param name="headers">Optional message headers.</param>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="messageId">Message identifier.</param>
    public MessageContext(
        TPayload payload,
        IDictionary<string, object>? headers = null,
        string? correlationId = null,
        string? messageId = null)
    {
        Payload = payload;
        Headers = headers;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        MessageId = messageId ?? Guid.NewGuid().ToString();
    }

    /// <summary>Message payload.</summary>
    public TPayload Payload { get; init; }

    /// <summary>Optional message headers.</summary>
    public IDictionary<string, object>? Headers { get; init; }

    /// <summary>Identifier used to correlate related messages.</summary>
    public string CorrelationId { get; init; }

    /// <summary>Unique identifier for this message.</summary>
    public string MessageId { get; init; }
}
