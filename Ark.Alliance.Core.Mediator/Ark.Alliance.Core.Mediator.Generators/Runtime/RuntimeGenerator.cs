using System.Reflection;
using System.Text;

namespace Ark.Alliance.Core.Mediator.Generators.Runtime;

/// <summary>
/// Runtime generator able to produce handler registrations dynamically.
/// </summary>
public sealed class RuntimeGenerator : IDisposable
{
    private readonly AdaptiveGenerationCache _cache;

    /// <summary>
    /// Initializes a new instance of <see cref="RuntimeGenerator"/>.
    /// </summary>
    public RuntimeGenerator(AdaptiveCacheConfiguration? config = null)
    {
        _cache = new AdaptiveGenerationCache(config ?? new AdaptiveCacheConfiguration(CacheMode.Memory));
    }

    /// <summary>
    /// Generates registration code for handlers found in the given assembly.
    /// </summary>
    public async Task<string> GenerateHandlerRegistrationsAsync(Assembly assembly)
    {
        var types = assembly.GetTypes().Where(IsHandlerType).ToArray();
        var key = $"runtime_{assembly.FullName}";
        if (await _cache.TryGetAsync(key))
            return string.Empty;

        var regs = types.SelectMany(GetRegistrations).Distinct().ToArray();
        var code = GenerateRegistrationCode(regs);
        await _cache.SetAsync(key, ComputeHash(code));
        return code;
    }

    /// <summary>
    /// Compiles and executes generated code.
    /// </summary>
    public async Task<T> CompileAndExecuteAsync<T>(string code, string methodName, object[] args)
    {
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "runtime",
            new[] { tree },
            new[]
            {
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(IServiceProvider).Assembly.Location)
            },
            new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        using var ms = new System.IO.MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
            throw new InvalidOperationException("Compilation failed");

        ms.Position = 0;
        var asm = Assembly.Load(ms.ToArray());
        var type = asm.GetTypes().First();
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        var value = method?.Invoke(null, args);
        return await Task.FromResult((T)value!);
    }

    private static bool IsHandlerType(Type type)
        => type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(IsHandlerInterface);

    private static bool IsHandlerInterface(Type iface)
        => iface.IsGenericType && iface.Name switch
        {
            "ICommandHandler`2" => true,
            "IQueryHandler`2" => true,
            "IEventHandler`1" => true,
            "IStreamRequestHandler`2" => true,
            _ => false
        };

    private static IEnumerable<(Type Interface, Type Implementation)> GetRegistrations(Type type)
    {
        return type.GetInterfaces()
            .Where(IsHandlerInterface)
            .Select(i => (i, type));
    }

    private static string GenerateRegistrationCode(IEnumerable<(Type Interface, Type Implementation)> regs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("public static class RuntimeGeneratedRegistrations");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection RegisterHandlers(this IServiceCollection services)");
        sb.AppendLine("    {");
        foreach (var (iface, impl) in regs)
            sb.AppendLine($"        services.AddTransient(typeof({iface.FullName}), typeof({impl.FullName}));");
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string ComputeHash(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <inheritdoc/>
    public void Dispose() => _cache.Dispose();
}