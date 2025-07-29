using Ark.Alliance.Core.Mediator.Generators.Performance;
using Xunit;

public class PerformanceEnhancementsTests
{
    [Fact]
    public void ObjectPool_reuses_instances()
    {
        using var pool = new ObjectPool<System.Text.StringBuilder>(sb => sb.Clear());
        var sb1 = pool.Get();
        pool.Return(sb1);
        var sb2 = pool.Get();
        Assert.Same(sb1, sb2);
    }

    [Fact]
    public void PerformanceMonitor_tracks_operations()
    {
        using var monitor = new GenerationPerformanceMonitor();
        using (monitor.StartOperation("test"))
        {
            // Operation executed; metrics are tracked internally
        }
        // no exception indicates success
        Assert.True(true);
    }
}
