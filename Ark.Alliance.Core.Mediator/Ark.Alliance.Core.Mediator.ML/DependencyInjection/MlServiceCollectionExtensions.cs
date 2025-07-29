using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Ark.Alliance.Core.Mediator.Messaging;

namespace Ark.Alliance.Core.Mediator.ML;

/// <summary>
/// Registration helpers for ML middleware and services.
/// </summary>
public static class MlServiceCollectionExtensions
{
    /// <summary>
    /// Registers ML telemetry middleware and background service.
    /// </summary>
    public static IServiceCollection AddMlTelemetry(this IServiceCollection services, MlTelemetryOptions options)
    {
        var channel = Channel.CreateUnbounded<byte[]>();
        services.AddSingleton(channel);
        services.AddSingleton(options);
        services.AddSingleton<IEventHubProducer, EventHubProducerWrapper>();
        services.AddHostedService<EventHubTelemetryService>();
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(MlCommandMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(MlQueryMiddleware<,>));
        return services;
    }
}
