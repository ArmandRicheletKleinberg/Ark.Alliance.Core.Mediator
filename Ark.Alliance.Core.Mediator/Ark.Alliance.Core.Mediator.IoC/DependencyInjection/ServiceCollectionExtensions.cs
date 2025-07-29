using Ark.Alliance.Core.Mediator.Messaging;
using Ark.Alliance.Core.Mediator.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Resilience;
using Polly;
using System.Reflection;
using System.Text.Json;
namespace Ark.Alliance.Core.Mediator.IoC;

/// <summary>
/// Extension helpers to configure Ark.Alliance.Core.Mediator messaging and broker services.
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Methods (Public)

    /// <summary>
    /// Registers Ark.Alliance.Core.Mediator dispatcher, handlers and broker based on configuration.
    /// </summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="assemblies">Assemblies containing CQRS handlers.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddArkCqrs(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var section = configuration.GetSection("Ark:Messaging");
        services.Configure<ArkMessagingOptions>(section);
        var opts = section.Get<ArkMessagingOptions>() ?? new();
        services.AddArkMessaging(opts, assemblies);

        if (opts.EnableLogging)
            services.AddCqrsLogging();

        if (opts.EnableRetry)
            services.AddRetryCommandMiddleware(o => o.RetryCount = opts.RetryCount);

        if (opts.EnablePolly)
        {
            var policy = Policy.Handle<Exception>().RetryAsync(opts.PollyRetryCount);
            services.AddPollyResilience(policy);
        }

        if (opts.EnablePipeline)
        {
            services.AddSingleton(new ResiliencePipelineProvider(opts.PipelineRetryCount));
            services.AddResiliencePipeline();
        }

        if (string.Equals(opts.EventPublisher, "Sequential", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton(typeof(IEventPublisher<>), typeof(SequentialEventPublisher<>));

        UseConfiguredBroker(services, configuration);
        return services;
    }

    #endregion Methods (Public)

    #region Methods (Private)

    /// <summary>
    /// Loads the configured broker assembly and registers its services if present.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">Application configuration containing broker details.</param>
    private static void UseConfiguredBroker(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Ark:Messaging");
        var opts = section.Get<ArkMessagingOptions>() ?? new();
        var asmName = $"Ark.Net.{opts.DefaultBroker}";
        try
        {
            var asm = Assembly.Load(asmName);
            var registrarType = asm.GetTypes().FirstOrDefault(t => typeof(IArkBrokerRegistrar).IsAssignableFrom(t));
            if (registrarType != null)
            {
                var registrar = (IArkBrokerRegistrar)Activator.CreateInstance(registrarType)!;
                if (opts.Brokers.TryGetValue(opts.DefaultBroker, out var cfgElem))
                {
                    using var doc = JsonDocument.Parse(cfgElem.GetRawText());
                    var cfgSection = doc.RootElement.Deserialize<Dictionary<string, object?>>() ?? new();
                    var providerData = cfgSection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());
                    var providerSection = new ConfigurationBuilder()
                        .AddInMemoryCollection(providerData!)
                        .Build()
                        .GetSection(string.Empty);
                    registrar.Register(services, providerSection);
                }
                else
                {
                    registrar.Register(services, section.GetSection("Brokers").GetSection(opts.DefaultBroker));
                }
            }
        }
        catch
        {
            // broker optional
        }
    }

    #endregion Methods (Private)
}
