using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using Ark.Alliance.Core.Mq.RabbitMq.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Resilience;

namespace Ark.Alliance.Core.Mq.RabbitMq;

/// <summary>
/// Extensions to register RabbitMQ services.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtension
{
    /// <summary>
    /// Adds RabbitMQ support using configuration.
    /// <code>
    /// services.AddRabbitMq(Configuration);
    /// </code>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration source.</param>
    /// <summary>Adds RabbitMQ support using configuration.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Configuration root containing RabbitMQ settings.</param>
    /// <param name="sectionKey">Configuration section key.</param>
    public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration, string sectionKey = "RabbitMq")
    {
        var section = configuration.GetSection(sectionKey);
        services.Configure<RabbitMqSettings>(section);

        var opts = section.Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        services.AddSingleton(new ResiliencePipelineProvider(opts.RetryCount));

        if (opts.EnableOpenTelemetry)
        {
            services.AddOpenTelemetry()
                .WithMetrics(b =>
                {
                    b.AddMeter(RabbitMqMetrics.MeterName);
                });
        }

        services.AddSingleton<IConnectionManager, RabbitMqConnection>();
        services.AddSingleton<IChannelPool, RabbitMqChannelPool>();
        services.AddTransient<RabbitMqPublisher>();
        services.AddTransient<IBrokerProducer, RabbitMqPublisher>();
        services.AddTransient<RabbitMqConsumer>();
        services.AddTransient<IBrokerConsumer, RabbitMqBrokerConsumer>();
    }
}
