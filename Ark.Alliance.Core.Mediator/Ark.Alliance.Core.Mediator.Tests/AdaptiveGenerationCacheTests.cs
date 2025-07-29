using Ark.Alliance.Core.Mediator.Generators;
using Xunit;

public class AdaptiveGenerationCacheTests
{
    [Fact]
    public async Task TryGet_updates_access_count()
    {
        var cache = new AdaptiveGenerationCache(new AdaptiveCacheConfiguration(CacheMode.Memory));
        await cache.SetAsync("k", "h");
        var entry1 = cache.GetEntry("k")!;
        Assert.Equal(1, entry1.AccessCount);
        Assert.Equal(entry1.CreatedAt, entry1.LastAccessed);

        var hit = await cache.TryGetAsync("k");
        Assert.True(hit);
        var entry2 = cache.GetEntry("k")!;
        Assert.Equal(2, entry2.AccessCount);
        Assert.True(entry2.LastAccessed > entry1.LastAccessed);
    }

    [Fact]
    public async Task Expired_entry_is_evicted()
    {
        var cache = new AdaptiveGenerationCache(new AdaptiveCacheConfiguration(CacheMode.Memory, null, TimeSpan.FromMilliseconds(50)));
        await cache.SetAsync("k", "h");
        await Task.Delay(80);
        var hit = await cache.TryGetAsync("k");
        Assert.False(hit);
    }
}
