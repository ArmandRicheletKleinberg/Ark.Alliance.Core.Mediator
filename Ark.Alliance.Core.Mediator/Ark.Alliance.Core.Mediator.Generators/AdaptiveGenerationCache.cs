using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable RS1035

namespace Ark.Alliance.Core.Mediator.Generators;

/// <summary>
/// Adaptive cache storing generation hashes in memory and optionally on disk.
/// </summary>
public sealed class AdaptiveGenerationCache : IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _memory = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly Timer _cleanup;
    private readonly AdaptiveCacheConfiguration _config;

    /// <summary>
    /// Initializes a new instance of <see cref="AdaptiveGenerationCache"/>.
    /// </summary>
    public AdaptiveGenerationCache(AdaptiveCacheConfiguration config)
    {
        _config = config;
        _cleanup = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        if (_config.Mode.HasFlag(CacheMode.File))
            _ = LoadFromFileAsync(CancellationToken.None);
    }

    /// <summary>
    /// Attempts to get an entry from the cache.
    /// </summary>
    public async ValueTask<bool> TryGetAsync(string key, CancellationToken ct = default)
    {
        if (_memory.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                entry.LastAccessed = DateTime.UtcNow;
                entry.AccessCount++;
                return true;
            }
            _memory.TryRemove(key, out _);
        }

        if (_config.Mode.HasFlag(CacheMode.File))
        {
            var fromFile = await LoadFromFileAsync(key, ct);
            if (fromFile != null && !fromFile.IsExpired)
            {
                fromFile.LastAccessed = DateTime.UtcNow;
                fromFile.AccessCount++;
                _memory.AddOrUpdate(key, fromFile, (_, _) => fromFile);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Stores a hash in the cache.
    /// </summary>
    public async ValueTask SetAsync(string key, string hash, Dictionary<string, object>? metadata = null, CancellationToken ct = default)
    {
        var entry = new CacheEntry
        {
            Key = key,
            Hash = hash,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow,
            AccessCount = 1,
            ExpiresAt = _config.ExpirationTime.HasValue ? DateTime.UtcNow.Add(_config.ExpirationTime.Value) : null,
            Metadata = metadata ?? new(),
        };

        _memory.AddOrUpdate(key, entry, (_, _) => entry);
        if (_config.Mode.HasFlag(CacheMode.File))
            await SaveToFileAsync(entry, ct);
    }

    /// <summary>
    /// Retrieves the cache entry when present. Mainly used for testing.
    /// </summary>
    public CacheEntry? GetEntry(string key)
    {
        _memory.TryGetValue(key, out var entry);
        return entry;
    }

    private async Task LoadFromFileAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_config.FilePath) || !File.Exists(_config.FilePath))
            return;

        await _fileLock.WaitAsync(ct);
        try
        {
            var json = await File.ReadAllTextAsync(_config.FilePath, ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json);
            if (data != null)
            {
                foreach (var (k, v) in data)
                {
                    if (!v.IsExpired)
                        _memory[k] = v;
                }
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<CacheEntry?> LoadFromFileAsync(string key, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_config.FilePath) || !File.Exists(_config.FilePath))
            return null;

        await _fileLock.WaitAsync(ct);
        try
        {
            var json = await File.ReadAllTextAsync(_config.FilePath, ct);
            var data = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json);
            if (data != null && data.TryGetValue(key, out var entry))
                return entry;
        }
        finally
        {
            _fileLock.Release();
        }

        return null;
    }

    private async Task SaveToFileAsync(CacheEntry entry, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_config.FilePath))
            return;

        await _fileLock.WaitAsync(ct);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_config.FilePath)!);
            Dictionary<string, CacheEntry> data = new();
            if (File.Exists(_config.FilePath))
            {
                var json = await File.ReadAllTextAsync(_config.FilePath, ct);
                data = JsonSerializer.Deserialize<Dictionary<string, CacheEntry>>(json) ?? new();
            }
            data[entry.Key] = entry;
            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(_config.FilePath, JsonSerializer.Serialize(data, options), ct);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private void CleanupExpired(object? state)
    {
        foreach (var (k, v) in _memory)
        {
            if (v.IsExpired ||
                (_config.MaxMemoryEntries > 0 && _memory.Count > _config.MaxMemoryEntries && v.AccessCount < 2))
            {
                _memory.TryRemove(k, out _);
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cleanup.Dispose();
        _fileLock.Dispose();
    }
}

/// <summary>
/// Cache entry with optional expiration.
/// </summary>
public sealed record CacheEntry
{
    public string Key { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
        = DateTime.UtcNow;
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    public int AccessCount { get; set; }
        = 0;
    public DateTime? ExpiresAt { get; init; }
        = null;
    public Dictionary<string, object> Metadata { get; init; } = new();

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

/// <summary>
/// Available cache modes.
/// </summary>
[Flags]
public enum CacheMode
{
    None = 0,
    Memory = 1,
    File = 2,
    Hybrid = Memory | File,
}

/// <summary>
/// Configuration for <see cref="AdaptiveGenerationCache"/>.
/// </summary>
public record AdaptiveCacheConfiguration(
    CacheMode Mode = CacheMode.Memory,
    string? FilePath = null,
    TimeSpan? ExpirationTime = null,
    int MaxMemoryEntries = 1000);
#pragma warning restore RS1035
