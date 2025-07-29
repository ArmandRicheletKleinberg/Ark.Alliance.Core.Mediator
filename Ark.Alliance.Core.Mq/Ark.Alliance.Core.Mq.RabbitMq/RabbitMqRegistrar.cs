using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Registrar enabling automatic RabbitMQ service registration through Ark IoC.
/// </summary>
public sealed class RabbitMqRegistrar : IArkBrokerRegistrar
{
    /// <inheritdoc />
    public void Register(IServiceCollection services, IConfigurationSection config)
    {
        services.AddRabbitMq(config, string.Empty);
    }
}
