using System;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core;

namespace Microsoft.Extensions.Resilience;

/// <summary>
/// Minimal retry options used when the Microsoft resilience library is not available.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the number of retry attempts to perform.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Simplified provider that creates <see cref="ResiliencePipeline"/> instances
/// for environments without the official package.
/// </summary>
public class ResiliencePipelineProvider(int retryAttempts = 3)
{
    private readonly int _retryAttempts = retryAttempts;

    public ResiliencePipeline GetPipeline(string name) => new(_retryAttempts);

    public ResiliencePipeline<TResult> GetPipeline<TResult>(string name) => new(_retryAttempts);
}

/// <summary>
/// Simple retry pipeline executing asynchronous actions a fixed number of times.
/// </summary>
public class ResiliencePipeline(int retryAttempts = 3)
{
    private readonly int _retryAttempts = retryAttempts;

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await action(cancellationToken);
                return;
            }
            catch when (attempt < _retryAttempts)
            {
            }
        }
    }
}

/// <summary>
/// Generic version of <see cref="ResiliencePipeline"/> returning a result value.
/// </summary>
public class ResiliencePipeline<TResult>(int retryAttempts = 3)
{
    private readonly int _retryAttempts = retryAttempts;

    public async Task<Result<TResult>> ExecuteAsync(
        Func<CancellationToken, Task<Result<TResult>>> action,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch when (attempt < _retryAttempts)
            {
            }
        }
    }
}

    /// <summary>
    /// Builder used to configure and create <see cref="ResiliencePipeline{TResult}"/> instances.
    /// </summary>
    public class ResiliencePipelineBuilder<TResult>
    {
        private int _retryAttempts;
        /// <summary>
        /// Adds simple retry behavior to the pipeline being built.
        /// </summary>
        /// <param name="configure">Action configuring retry options.</param>
        /// <returns>The current builder instance.</returns>
        public ResiliencePipelineBuilder<TResult> AddRetry(Action<RetryOptions> configure)
        {
            var opts = new RetryOptions();
            configure(opts);
            _retryAttempts = opts.MaxRetryAttempts;
            return this;
        }

        /// <summary>
        /// Builds the configured <see cref="ResiliencePipeline{TResult}"/>.
        /// </summary>
        /// <returns>A new pipeline instance.</returns>
        public ResiliencePipeline<TResult> Build() => new(_retryAttempts);
    }

