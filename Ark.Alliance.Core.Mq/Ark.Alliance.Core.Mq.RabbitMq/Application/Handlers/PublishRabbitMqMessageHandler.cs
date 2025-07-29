

using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;


namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Handles <see cref="PublishRabbitMqMessageCommand{TMessage}"/> by delegating to <see cref="RabbitMqPublisher"/>.
/// </summary>
/// <typeparam name="TMessage">Type of the message payload.</typeparam>
public class PublishRabbitMqMessageHandler<TMessage> : ICommandHandler<PublishRabbitMqMessageCommand<TMessage>, Unit> where TMessage : class
{
    private readonly RabbitMqPublisher _publisher;

    /// <summary>
    /// Creates a new handler instance.
    /// </summary>
    /// <param name="publisher">Publisher used to send messages.</param>
    public PublishRabbitMqMessageHandler(RabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc />
    public async Task<Result<Unit>> HandleAsync(PublishRabbitMqMessageCommand<TMessage> request, CancellationToken cancellationToken = default)
    {
        var result = await _publisher.PublishAsync(request.Exchange, request.RoutingKey, request.Message, token: cancellationToken);
        return result.IsSuccess ? Result<Unit>.Success.WithData(Unit.Value) : new Result<Unit>(result);
    }
}
