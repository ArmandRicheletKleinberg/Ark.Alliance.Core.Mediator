using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ark.Alliance.Core.Mediator.Generators.Performance;

/// <summary>
/// Simple performance monitor able to track operation duration and item counts.
/// </summary>
public sealed class GenerationPerformanceMonitor : IDisposable
{
    private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
    private readonly Timer _timer;

    public GenerationPerformanceMonitor()
    {
        _timer = new Timer(Report, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Starts tracking a new operation.
    /// </summary>
    public IDisposable StartOperation(string name) => new OperationTracker(this, name);

    internal void Record(string name, TimeSpan duration, int itemCount)
    {
        _metrics.AddOrUpdate(name,
            new PerformanceMetrics(name, duration, 1, itemCount),
            (_, existing) => existing with
            {
                TotalDuration = existing.TotalDuration + duration,
                CallCount = existing.CallCount + 1,
                ItemCount = existing.ItemCount + itemCount
            });
    }

    private void Report(object? state)
    {
#if DEBUG
        foreach (var (n, m) in _metrics)
        {
            var avg = m.TotalDuration.TotalMilliseconds / m.CallCount;
            Debug.WriteLine($"[PERF] {n}: {m.CallCount} calls avg {avg:F2}ms items {m.ItemCount}");
        }
#endif
    }

    public void Dispose() => _timer.Dispose();

    private sealed class OperationTracker : IDisposable
    {
        private readonly GenerationPerformanceMonitor _monitor;
        private readonly string _name;
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private int _count;

        public OperationTracker(GenerationPerformanceMonitor monitor, string name)
        {
            _monitor = monitor;
            _name = name;
        }

        public void AddItems(int count) => _count += count;

        public void Dispose()
        {
            _sw.Stop();
            _monitor.Record(_name, _sw.Elapsed, _count);
        }
    }
}

public record PerformanceMetrics(string OperationName, TimeSpan TotalDuration, int CallCount, int ItemCount);

/// <summary>
/// Very small object pool used by the generator helpers.
/// </summary>
public sealed class ObjectPool<T> : IDisposable where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly Action<T>? _reset;
    private bool _disposed;

    public ObjectPool(Action<T>? reset = null)
    {
        _reset = reset;
    }

    public T Get() => _objects.TryDequeue(out var item) ? item! : new T();

    public void Return(T item)
    {
        if (_disposed) return;
        _reset?.Invoke(item);
        _objects.Enqueue(item);
    }

    public void Dispose()
    {
        _disposed = true;
        while (_objects.TryDequeue(out var item))
        {
            if (item is IDisposable d)
                d.Dispose();
        }
    }
}

/// <summary>
/// Helper utilities for pooled StringBuilder and quick hashing.
/// </summary>
public static class PerformanceUtils
{
    private static readonly ObjectPool<System.Text.StringBuilder> _sbPool = new(reset: sb => sb.Clear());

    public static PooledStringBuilder GetStringBuilder() => new(_sbPool);

    public static string ComputeFastHash(ReadOnlySpan<char> input)
    {
        unchecked
        {
            int hash = 23;
            foreach (var c in input)
                hash = (hash * 31) + c;
            return hash.ToString("X8");
        }
    }
}

/// <summary>
/// Wrapper struct ensuring pooled StringBuilder is returned.
/// </summary>
public readonly struct PooledStringBuilder : IDisposable
{
    private readonly ObjectPool<System.Text.StringBuilder> _pool;
    public System.Text.StringBuilder Builder { get; }

    internal PooledStringBuilder(ObjectPool<System.Text.StringBuilder> pool)
    {
        _pool = pool;
        Builder = pool.Get();
    }

    public void Dispose() => _pool.Return(Builder);

    public override string ToString() => Builder.ToString();
}
