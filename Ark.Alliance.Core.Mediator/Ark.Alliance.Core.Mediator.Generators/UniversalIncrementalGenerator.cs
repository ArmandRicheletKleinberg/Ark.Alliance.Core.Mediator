using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Ark.Alliance.Core.Mediator.Generators;

/// <summary>
/// Provides an in-memory and optional file based cache used by source
/// generators to skip regeneration when nothing changed.
/// </summary>
internal sealed class GenerationCache
{
    #region Fields

    /// <summary>Selected cache mode.</summary>
    private readonly RegistrationCacheMode _mode;

    /// <summary>Optional cache file location.</summary>
    private readonly string? _filePath;

    /// <summary>In-memory cache store.</summary>
    private readonly Dictionary<string, string> _memory = new();

    #endregion Fields

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationCache"/> class.
    /// </summary>
    /// <param name="mode">The cache strategy to use.</param>
    /// <param name="filePath">Optional path of the persistent cache file.</param>
    public GenerationCache(RegistrationCacheMode mode, string? filePath)
    {
        _mode = mode;
        _filePath = filePath;

        if (_mode is RegistrationCacheMode.File or RegistrationCacheMode.Hybrid)
        {
            LoadFileCache();
        }
    }

    /// <summary>
    /// Checks whether the provided hash matches the cache entry for the
    /// specified key.
    /// </summary>
    /// <param name="key">Cache entry key.</param>
    /// <param name="hash">Content hash to compare.</param>
    /// <returns><c>true</c> when the cached hash equals <paramref name="hash"/>.</returns>
    public bool IsUpToDate(string key, string hash)
    {
        if (_mode == RegistrationCacheMode.None)
            return false;

        if (_memory.TryGetValue(key, out var existing) && existing == hash)
            return true;

        return false;
    }

    /// <summary>
    /// Stores a new hash value in the cache and persists it when required.
    /// </summary>
    /// <param name="key">Entry key.</param>
    /// <param name="hash">Computed hash.</param>
    public void Update(string key, string hash)
    {
        if (_mode == RegistrationCacheMode.None)
            return;

        _memory[key] = hash;

        if (_mode is RegistrationCacheMode.File or RegistrationCacheMode.Hybrid)
            SaveFileCache();
    }

    /// <summary>
    /// Loads cache entries from disk when a file path is configured.
    /// </summary>
    private void LoadFileCache()
    {
        if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            return;

        var result = Result<Dictionary<string, string>>.SafeExecute(() =>
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            return Result<Dictionary<string, string>>.Success.WithData(data);
        });

        if (result.IsSuccess && result.HasData)
        {
            foreach (var (k, v) in result.Data)
                _memory[k] = v;
        }

#if DEBUG
        Debug.WriteLine($"[GeneratorCache] Loaded {_memory.Count} entries");
#endif
    }

    /// <summary>
    /// Persists the current cache to disk if a file path is configured.
    /// </summary>
    private void SaveFileCache()
    {
        if (string.IsNullOrEmpty(_filePath))
            return;

        var result = Result.SafeExecute(() =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var json = JsonSerializer.Serialize(_memory);
            File.WriteAllText(_filePath, json);
            return Result.Success;
        });

#if DEBUG
        if (result.IsSuccess)
            Debug.WriteLine($"[GeneratorCache] Saved {_memory.Count} entries");
        else
            Debug.WriteLine($"[GeneratorCache] Save failed: {result.Exception?.Message}");
#endif
    }
}


/// <summary>
/// Configuration for <see cref="GenerationCache"/>.
/// </summary>
public record GenerationCacheConfiguration(RegistrationCacheMode Mode, string? FilePath);

/// <summary>
/// Represents processed generator inputs with an associated content hash.
/// </summary>
public record GenerationInput<T>(ImmutableArray<T> Items, string ContentHash, Dictionary<string, string>? Metadata = null)
{
    public bool IsEmpty => Items.IsDefaultOrEmpty;
}

/// <summary>
/// Represents a single generated source file.
/// </summary>
public record GenerationOutput(string FileName, string Content)
{
    public SourceText ToSourceText() => SourceText.From(Content, Encoding.UTF8);
}

/// <summary>
/// Base class providing common source generator plumbing with caching support.
/// </summary>
/// <typeparam name="TInput">Type produced by the syntax provider.</typeparam>
/// <typeparam name="TOutput">Processed input type used during generation.</typeparam>
public abstract class UniversalIncrementalGenerator<TInput, TOutput> : IIncrementalGenerator
    where TOutput : class
{
    /// <summary>Display name used for diagnostics and cache keys.</summary>
    protected abstract string GeneratorName { get; }

    /// <summary>
    /// Collects relevant syntax nodes for processing.
    /// </summary>
    /// <param name="context">Generator initialization context.</param>
    protected virtual IncrementalValuesProvider<TInput> CollectInputs(IncrementalGeneratorInitializationContext context) => throw new NotImplementedException();

    /// <summary>
    /// Converts collected syntax into a serializable model.
    /// </summary>
    /// <param name="inputs">Collected syntax elements.</param>
    /// <param name="compilation">Current compilation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected virtual GenerationInput<TOutput> ProcessInputs(
        ImmutableArray<TInput> inputs,
        Compilation compilation,
        CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <summary>
    /// Generates source files from processed input.
    /// </summary>
    protected virtual IEnumerable<GenerationOutput> GenerateCode(GenerationInput<TOutput> input) => Enumerable.Empty<GenerationOutput>();

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var config = context.AnalyzerConfigOptionsProvider.Select((p, _) => ReadConfig(p.GlobalOptions));

        var collected = CollectInputs(context)
            .Collect()
            .Combine(context.CompilationProvider)
            .Combine(config);

        context.RegisterSourceOutput(collected, (ctx, tuple) =>
        {
            var inputs = tuple.Left.Left;
            var compilation = tuple.Left.Right;
            var cfg = tuple.Right;
            var processed = ProcessInputs(inputs, compilation, ctx.CancellationToken);
            Generate(ctx, processed, cfg);
        });
    }

    /// <summary>
    /// Reads generator configuration from .editorconfig or environment variables.
    /// </summary>
    private static GenerationCacheConfiguration ReadConfig(Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions options)
    {
        options.TryGetValue("build_property.ArkMediatorGeneratorCacheMode", out var modeStr);
        if (string.IsNullOrEmpty(modeStr))
            modeStr = Environment.GetEnvironmentVariable("ARK_MEDIATOR_GENERATOR_CACHE_MODE");

        if (!Enum.TryParse<RegistrationCacheMode>(modeStr, true, out var mode))
            mode = RegistrationCacheMode.Memory;

        options.TryGetValue("build_property.ArkMediatorGeneratorCacheFile", out var path);
        if (string.IsNullOrEmpty(path))
            path = Environment.GetEnvironmentVariable("ARK_MEDIATOR_GENERATOR_CACHE_FILE");

        return new GenerationCacheConfiguration(mode, path);
    }

    /// <summary>
    /// Handles cache checks and emits generated source when required.
    /// </summary>
    private void Generate(SourceProductionContext context, GenerationInput<TOutput> input, GenerationCacheConfiguration config)
    {
        if (input.IsEmpty)
            return;

        var cache = new GenerationCache(config.Mode, config.FilePath);
        if (cache.IsUpToDate(GeneratorName, input.ContentHash))
            return;

        List<GenerationOutput> outputs;
        try
        {
            outputs = GenerateCode(input).ToList();
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    id: $"{GeneratorName}001",
                    title: GeneratorName,
                    messageFormat: $"{GeneratorName} failed: {ex.Message}",
                    category: "Generator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None));
            return;
        }

        if (outputs.Count == 0)
            return;

        var finalHash = ComputeHash(string.Join("", outputs.Select(o => o.Content)));
        cache.Update(GeneratorName, finalHash);

        foreach (var output in outputs)
            context.AddSource(output.FileName, output.ToSourceText());
    }

    /// <summary>
    /// Computes a deterministic SHA-256 hash for the given string.
    /// </summary>
    /// <param name="value">Value to hash.</param>
    protected static string ComputeHash(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Computes a hash for a collection of inputs.
    /// </summary>
    protected static string ComputeIncrementalHash(IEnumerable<object> inputs)
    {
        var combined = string.Join("|", inputs.Select(i => i?.ToString() ?? "null"));
        return ComputeHash(combined);
    }
}