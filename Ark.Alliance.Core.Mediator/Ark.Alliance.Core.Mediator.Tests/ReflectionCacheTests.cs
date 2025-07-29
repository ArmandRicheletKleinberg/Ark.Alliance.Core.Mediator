using Ark.Alliance.Core;
using Ark.Alliance.Core.Mediator.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Validates reflection cache strategies used for handler registration.
/// </summary>
public class ReflectionCacheTests
{
    #region Records
    /// <summary>
    /// Command used to verify reflection caching.
    /// </summary>
    public record CacheCommand() : ICommand<string>;
    #endregion Records

    #region Handlers
    /// <summary>
    /// Simple handler returning an "ok" string.
    /// </summary>
    public class CacheHandler : ICommandHandler<CacheCommand, string>
    {
        public Task<Result<string>> HandleAsync(CacheCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<string>.Success.WithData("ok"));
    }

    #endregion Handlers

    #region Methods (Tests)

    /// <summary>
    /// Registers handlers twice and verifies the memory cache prevents duplicates.
    /// </summary>
    [Fact]
    public async Task Memory_cache_registers_once()
    {
        var options = new ArkMessagingOptions
        {
            HandlerRegistration = HandlerRegistrationMode.Reflection,
            ReflectionCache = RegistrationCacheMode.Memory
        };

        var services = new ServiceCollection();
        services.AddArkMessaging(options, typeof(ReflectionCacheTests).Assembly);
        services.AddArkMessaging(options, typeof(ReflectionCacheTests).Assembly);
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IArkDispatcher>();

        var result = await dispatcher.SendAsync<CacheCommand, string>(new CacheCommand());
        Assert.Equal(ResultStatus.Success, result.Status);
    }

    /// <summary>
    /// Confirms file-based caching keeps registrations across service instances.
    /// </summary>
    [Fact]
    public async Task File_cache_persists_between_calls()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var options = new ArkMessagingOptions
        {
            HandlerRegistration = HandlerRegistrationMode.Reflection,
            ReflectionCache = RegistrationCacheMode.File,
            ReflectionCacheFile = path
        };

        var services1 = new ServiceCollection();
        services1.AddArkMessaging(options, typeof(ReflectionCacheTests).Assembly);
        var provider1 = services1.BuildServiceProvider();
        var dispatcher1 = provider1.GetRequiredService<IArkDispatcher>();
        var result1 = await dispatcher1.SendAsync<CacheCommand, string>(new CacheCommand());
        Assert.Equal(ResultStatus.Success, result1.Status);

        var services2 = new ServiceCollection();
        services2.AddArkMessaging(options, typeof(ReflectionCacheTests).Assembly);
        var provider2 = services2.BuildServiceProvider();
        var dispatcher2 = provider2.GetRequiredService<IArkDispatcher>();
        var result2 = await dispatcher2.SendAsync<CacheCommand, string>(new CacheCommand());
        Assert.Equal(ResultStatus.Success, result2.Status);

        File.Delete(path);
    }

    #endregion Methods (Tests)
}
