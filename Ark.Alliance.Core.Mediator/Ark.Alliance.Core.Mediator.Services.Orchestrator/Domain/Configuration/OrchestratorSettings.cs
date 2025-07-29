namespace Ark.Alliance.Core.Mediator.Services.Orchestrator;

/// <summary>
/// Root settings describing the orchestrator configuration.
/// </summary>
public class OrchestratorSettings
{
    #region Properties
    /// <summary>Main application parameters.</summary>
    public MainApplicationSettings MainApplication { get; set; } = new();

    /// <summary>Available broker configurations keyed by name.</summary>
    public Dictionary<string, BrokerSettings> Brokers { get; set; } = new();

    /// <summary>Services orchestrated by this application.</summary>
    public Dictionary<string, ServiceSettings> Services { get; set; } = new();

    /// <summary>Degree of parallelism when launching services.</summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>Enable health check endpoints for orchestrated services.</summary>
    public bool EnableHealthChecks { get; set; }

    /// <summary>Stop starting additional services when one fails.</summary>
    public bool StopOnError { get; set; }
    #endregion Properties
}

/// <summary>
/// Defines main application parameters for the orchestrator.
/// </summary>
public class MainApplicationSettings
{
    #region Properties
    /// <summary>Database connections used by the orchestrator.</summary>
    public Dictionary<string, DatabaseSettings> Databases { get; set; } = new();

    /// <summary>Authorization settings.</summary>
    public AuthorizationSettings Authorization { get; set; } = new();
    #endregion Properties
}

/// <summary>
/// Connection settings for a database.
/// </summary>
public class DatabaseSettings
{
    #region Properties
    /// <summary>The connection string.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Whether the database schema should be migrated.</summary>
    public bool Migrate { get; set; }
    #endregion Properties
}

/// <summary>
/// Settings controlling authorization.
/// </summary>
public class AuthorizationSettings
{
    #region Properties
    /// <summary>Type of authorization used.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Service responsible for authorization.</summary>
    public string Service { get; set; } = string.Empty;
    #endregion Properties
}

/// <summary>
/// Connection information for a broker.
/// </summary>
public class BrokerSettings
{
    #region Properties
    /// <summary>Broker host name.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Broker port.</summary>
    public int Port { get; set; }
    #endregion Properties
}

/// <summary>
/// Describes a service orchestrated by the application.
/// </summary>
public class ServiceSettings
{
    #region Properties
    /// <summary>Service version.</summary>
    public string Version { get; set; } = "1.0";

    /// <summary>Associated roles.</summary>
    public string[] Roles { get; set; } = Array.Empty<string>();

    /// <summary>Execution schedule.</summary>
    public ScheduleSettings Schedule { get; set; } = new();

    /// <summary>Logging configuration.</summary>
    public LoggerSettings Logger { get; set; } = new();

    /// <summary>Broker configuration name.</summary>
    public string Broker { get; set; } = string.Empty;

    /// <summary>Event configuration.</summary>
    public EventsSettings Events { get; set; } = new();

    /// <summary>Whether the service should be started.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Delay in seconds before starting the service.</summary>
    public int StartupDelaySeconds { get; set; }

    /// <summary>Startup must finish within this timeout in seconds. Zero disables the limit.</summary>
    public int StartupTimeoutSeconds { get; set; }

    /// <summary>Priority order for service startup. Lower numbers start first.</summary>
    public int Priority { get; set; }

    /// <summary>Number of parallel instances for this service.</summary>
    public int InstanceCount { get; set; } = 1;

    /// <summary>Number of retries when startup fails.</summary>
    public int RetryCount { get; set; }
    #endregion Properties
}

/// <summary>
/// Scheduling parameters for a service.
/// </summary>
public class ScheduleSettings
{
    #region Properties
    /// <summary>Interval between laps.</summary>
    public TimeSpan ScheduledLaps { get; set; }

    /// <summary>Local start time.</summary>
    public DateTime? ScheduledStartLocalTime { get; set; }

    /// <summary>UTC start time.</summary>
    public DateTime? ScheduledStartUtcTime { get; set; }
    #endregion Properties
}

/// <summary>
/// Configuration controlling logging behavior.
/// </summary>
public class LoggerSettings
{
    #region Properties
    /// <summary>Whether logging is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Minimum logging level.</summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>Enable metrics exporting.</summary>
    public bool EnableMetrics { get; set; }
    #endregion Properties
}

/// <summary>
/// Event publishing and subscription configuration.
/// </summary>
public class EventsSettings
{
    #region Properties
    /// <summary>Events to publish.</summary>
    public string[] Publish { get; set; } = Array.Empty<string>();

    /// <summary>Events to subscribe to.</summary>
    public SubscribeEvent[] Subscribe { get; set; } = Array.Empty<SubscribeEvent>();
    #endregion Properties
}

/// <summary>
/// Details of a subscribed event.
/// </summary>
public class SubscribeEvent
{
    #region Properties
    /// <summary>Event name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Subscription conditions.</summary>
    public string[] Conditions { get; set; } = Array.Empty<string>();
    #endregion Properties
}
