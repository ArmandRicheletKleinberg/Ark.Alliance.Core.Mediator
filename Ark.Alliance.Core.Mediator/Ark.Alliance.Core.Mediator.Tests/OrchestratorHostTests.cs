using Ark.Alliance.Core.Mediator.Services.Orchestrator;
using Xunit;

/// <summary>
/// Tests <see cref="OrchestratorHost"/> startup behaviours such as parallelism,
/// delays and retries.
/// </summary>
public class OrchestratorHostTests
{
    /// <summary>
    /// Ensures parallel execution limit is honoured and disabled services are skipped.
    /// </summary>
    [Fact]
    public async Task StartAsync_honors_parallelism_and_skips_disabled_services()
    {
        var settings = new OrchestratorSettings
        {
            MaxDegreeOfParallelism = 1,
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings(),
                ["B"] = new ServiceSettings { Enabled = false },
                ["C"] = new ServiceSettings()
            }
        };

        var started = new List<string>();
        var tcs = new TaskCompletionSource();
        var host = new OrchestratorHost(settings, async (svc, _) =>
        {
            started.Add(svc.Version);
            if (started.Count == 1)
                await tcs.Task;
        });

        var runTask = host.StartAsync();
        await Task.Delay(100);
        Assert.Single(started);
        tcs.SetResult();
        await runTask;
        Assert.Equal(2, started.Count);
    }

    /// <summary>
    /// Verifies <see cref="OrchestratorHost"/> waits before starting delayed services.
    /// </summary>
    [Fact]
    public async Task StartAsync_applies_startup_delay()
    {
        var settings = new OrchestratorSettings
        {
            MaxDegreeOfParallelism = 2,
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { StartupDelaySeconds = 1 },
                ["B"] = new ServiceSettings()
            }
        };

        var timestamps = new List<DateTime>();
        var host = new OrchestratorHost(settings, (svc, _) =>
        {
            timestamps.Add(DateTime.UtcNow);
            return Task.CompletedTask;
        });

        var before = DateTime.UtcNow;
        await host.StartAsync();

        Assert.Equal(2, timestamps.Count);
        Assert.Contains(timestamps, t => (t - before).TotalSeconds >= 1);
        Assert.Contains(timestamps, t => (t - before).TotalSeconds < 1);
    }

    /// <summary>
    /// Ensures services start in order of configured <see cref="ServiceSettings.Priority"/>.
    /// </summary>
    [Fact]
    public async Task StartAsync_orders_services_by_priority()
    {
        var settings = new OrchestratorSettings
        {
            MaxDegreeOfParallelism = 1,
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { Version = "A", Priority = 1 },
                ["B"] = new ServiceSettings { Version = "B", Priority = 0 }
            }
        };

        var order = new List<string>();
        var host = new OrchestratorHost(settings, (svc, _) =>
        {
            order.Add(svc.Version);
            return Task.CompletedTask;
        });

        await host.StartAsync();

        Assert.Equal(new[] { "B", "A" }, order);
    }

    /// <summary>
    /// Stops processing when a service throws and <see cref="OrchestratorSettings.StopOnError"/> is enabled.
    /// </summary>
    [Fact]
    public async Task StartAsync_stops_on_error_when_enabled()
    {
        var settings = new OrchestratorSettings
        {
            StopOnError = true,
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { Version = "A" },
                ["B"] = new ServiceSettings { Version = "B" }
            }
        };

        var started = new List<string>();
        var host = new OrchestratorHost(settings, (svc, _) =>
        {
            started.Add(svc.Version);
            if (svc.Version == "A")
                throw new InvalidOperationException("fail");
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => host.StartAsync());
        Assert.Single(started);
    }

    /// <summary>
    /// Throws <see cref="TaskCanceledException"/> when a service exceeds its startup timeout.
    /// </summary>
    [Fact]
    public async Task StartAsync_enforces_startup_timeout()
    {
        var settings = new OrchestratorSettings
        {
            StopOnError = true,
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { StartupTimeoutSeconds = 1 }
            }
        };

        var host = new OrchestratorHost(settings, async (svc, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
        });

        await Assert.ThrowsAsync<TaskCanceledException>(() => host.StartAsync());
    }

    /// <summary>
    /// Retries a service start the configured number of times until it succeeds.
    /// </summary>
    [Fact]
    public async Task StartAsync_retries_failed_startups()
    {
        var attempts = 0;
        var settings = new OrchestratorSettings
        {
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { RetryCount = 2 }
            }
        };

        var host = new OrchestratorHost(settings, (svc, _) =>
        {
            attempts++;
            if (attempts < 3)
                throw new InvalidOperationException();
            return Task.CompletedTask;
        });

        await host.StartAsync();

        Assert.Equal(3, attempts);
    }

    /// <summary>
    /// Starts the configured number of instances for a single service definition.
    /// </summary>
    [Fact]
    public async Task StartAsync_runs_multiple_instances()
    {
        var started = 0;
        var settings = new OrchestratorSettings
        {
            Services = new Dictionary<string, ServiceSettings>
            {
                ["A"] = new ServiceSettings { InstanceCount = 3 }
            }
        };

        var host = new OrchestratorHost(settings, (svc, _) =>
        {
            Interlocked.Increment(ref started);
            return Task.CompletedTask;
        });

        await host.StartAsync();

        Assert.Equal(3, started);
    }
}
