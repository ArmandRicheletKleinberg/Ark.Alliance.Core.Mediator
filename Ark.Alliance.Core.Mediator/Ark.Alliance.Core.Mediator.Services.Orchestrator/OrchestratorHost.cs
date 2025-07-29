namespace Ark.Alliance.Core.Mediator.Services.Orchestrator;

/// <summary>
/// Starts services defined in <see cref="OrchestratorSettings"/> respecting parallelism settings.
/// </summary>
public class OrchestratorHost
{
    private readonly OrchestratorSettings _settings;
    private readonly Func<ServiceSettings, CancellationToken, Task> _serviceStarter;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrchestratorHost"/> class.
    /// </summary>
    /// <param name="settings">Configuration describing services to run.</param>
    /// <param name="serviceStarter">Callback used to start each service.</param>
    public OrchestratorHost(OrchestratorSettings settings, Func<ServiceSettings, CancellationToken, Task> serviceStarter)
    {
        _settings = settings;
        _serviceStarter = serviceStarter;
    }

    /// <summary>
    /// Starts all enabled services respecting <see cref="OrchestratorSettings.MaxDegreeOfParallelism"/>.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var degree = _settings.MaxDegreeOfParallelism <= 0 ? Environment.ProcessorCount : _settings.MaxDegreeOfParallelism;
        using var semaphore = new SemaphoreSlim(degree);
        var tasks = new List<Task>();
        foreach (var svc in _settings.Services.Values.Where(s => s.Enabled).OrderBy(s => s.Priority))
        {
            var instances = svc.InstanceCount <= 0 ? 1 : svc.InstanceCount;
            for (var i = 0; i < instances; i++)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                var task = RunServiceAsync(svc, semaphore, cancellationToken);
                if (_settings.StopOnError)
                {
                    await task.ConfigureAwait(false);
                }
                else
                {
                    tasks.Add(task);
                }
            }
        }
        if (!_settings.StopOnError)
            await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RunServiceAsync(ServiceSettings svc, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (svc.StartupTimeoutSeconds > 0)
            cts.CancelAfter(TimeSpan.FromSeconds(svc.StartupTimeoutSeconds));
        if (svc.StartupDelaySeconds > 0)
            await Task.Delay(TimeSpan.FromSeconds(svc.StartupDelaySeconds), cancellationToken).ConfigureAwait(false);
        try
        {
            for (var attempt = 0; attempt <= svc.RetryCount; attempt++)
            {
                try
                {
                    await _serviceStarter(svc, cts.Token).ConfigureAwait(false);
                    return;
                }
                catch when (attempt < svc.RetryCount && !cancellationToken.IsCancellationRequested)
                {
                    if (svc.StartupDelaySeconds > 0)
                        await Task.Delay(TimeSpan.FromSeconds(svc.StartupDelaySeconds), cancellationToken).ConfigureAwait(false);
                }
            }

            if (_settings.StopOnError)
                throw new InvalidOperationException($"Service {svc.Version} failed after {svc.RetryCount + 1} attempts");
        }
        finally
        {
            semaphore.Release();
        }
    }
}
