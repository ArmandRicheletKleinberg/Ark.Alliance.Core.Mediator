using Ark.Alliance.Core.Mediator.Generators.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ark.Alliance.Core.Mediator.Generators.Hybrid;

/// <summary>
/// Extension methods for hybrid handler registration.
/// </summary>
public static class HybridGenerationSystem
{
    /// <summary>
    /// Registers handlers using compile-time registrations when available and
    /// falls back to runtime generation for missing handlers.
    /// </summary>
    public static IServiceCollection AddHybridHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        // invoke generated partial method if present without direct assembly reference
        var extType = Type.GetType(
            "Ark.Alliance.Core.Mediator.Messaging.IServiceCollectionExtensions, Ark.Alliance.Core.Mediator.Messaging",
            throwOnError: false);
        var method = extType?.GetMethod(
            "RegisterGeneratedHandlers",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { services });

        foreach (var asm in assemblies)
        {
            using var generator = new RuntimeGenerator();
            var code = generator.GenerateHandlerRegistrationsAsync(asm).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(code))
            {
                generator.CompileAndExecuteAsync<object>(code, "RegisterHandlers", new object[] { services }).GetAwaiter().GetResult();
            }
        }

        return services;
    }
}
