using System.Text.Json;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Configuration options for Ark messaging services.
/// </summary>
public class ArkMessagingOptions
{
    #region Properties (Public)

    /// <summary>
    /// Gets or sets the name of the default broker to use when publishing events.
    /// </summary>
    public string DefaultBroker { get; init; } = "RabbitMq";

    /// <summary>
    /// Gets or sets a value indicating whether logging middlewares
    /// should be registered automatically by <c>AddArkCqrs</c>.
    /// </summary>
    public bool EnableLogging { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether retry middleware
    /// should be added automatically by <c>AddArkCqrs</c>.
    /// </summary>
    public bool EnableRetry { get; init; }

    /// <summary>
    /// Gets or sets the number of retry attempts when
    /// <see cref="EnableRetry"/> is enabled. Defaults to 3.
    /// </summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether Polly based
    /// resilience middleware should be registered automatically by
    /// <c>AddArkCqrs</c>.
    /// </summary>
    public bool EnablePolly { get; init; }

    /// <summary>
    /// Gets or sets the number of retry attempts when
    /// <see cref="EnablePolly"/> is enabled. Defaults to 3.
    /// </summary>
    public int PollyRetryCount { get; init; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether the Microsoft
    /// resilience pipeline should be registered automatically by
    /// <c>AddArkCqrs</c>.
    /// </summary>
    public bool EnablePipeline { get; init; }

    /// <summary>
    /// Gets or sets the number of retry attempts when
    /// <see cref="EnablePipeline"/> is enabled. Defaults to 3.
    /// </summary>
    public int PipelineRetryCount { get; init; } = 3;

    /// <summary>
    /// Gets or sets the event publisher type. Use "Sequential" to
    /// invoke handlers in order, otherwise events are published in
    /// parallel.
    /// </summary>
    public string EventPublisher { get; init; } = "Parallel";

    /// <summary>
    /// Gets or sets how handlers are registered when invoking
    /// <see cref="IServiceCollectionExtensions.AddArkMessaging"/>.
    /// Defaults to <see cref="HandlerRegistrationMode.Both"/>.
    /// </summary>
    public HandlerRegistrationMode HandlerRegistration { get; init; } = HandlerRegistrationMode.Both;

    /// <summary>
    /// Gets or sets the caching strategy used when registering handlers via
    /// reflection. Defaults to <see cref="RegistrationCacheMode.None"/>.
    /// </summary>
    public RegistrationCacheMode ReflectionCache { get; init; } = RegistrationCacheMode.None;

    /// <summary>
    /// Gets or sets the file path used when <see cref="ReflectionCache"/> is set
    /// to <see cref="RegistrationCacheMode.File"/>.
    /// </summary>
    public string? ReflectionCacheFile { get; init; }

    /// <summary>
    /// Gets the raw broker configurations keyed by broker name.
    /// </summary>
    public Dictionary<string, JsonElement> Brokers { get; init; } = new();

    #endregion Properties (Public)
}
