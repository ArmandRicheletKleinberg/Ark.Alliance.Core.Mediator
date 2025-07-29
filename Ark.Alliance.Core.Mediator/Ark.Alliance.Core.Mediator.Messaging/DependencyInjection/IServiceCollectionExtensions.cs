using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Resilience;
using Polly;

namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Extension helpers to register Ark.Alliance.Core.Mediator.Messaging components into the DI container.
/// </summary>
public static partial class IServiceCollectionExtensions
{

    static partial void RegisterGeneratedHandlers(IServiceCollection services);



    #region Methods (Public)
    /// <summary>
    /// Registers Ark messaging dispatcher and all handlers found in the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assemblies">Assemblies containing CQRS handlers. When <c>null</c> or empty the calling assembly is scanned.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddArkMessaging(this IServiceCollection services, params Assembly[] assemblies)
        => AddArkMessaging(services, new ArkMessagingOptions(), assemblies);

    /// <summary>
    /// Registers Ark messaging dispatcher and handlers according to provided options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">Messaging configuration options.</param>
    /// <param name="assemblies">Assemblies containing CQRS handlers. When <c>null</c> or empty the calling assembly is scanned.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddArkMessaging(this IServiceCollection services, ArkMessagingOptions options, params Assembly[] assemblies)
    {
        // Ensure the default logging infrastructure is available for components
        services.AddLogging();
        services.AddTransient<IArkDispatcher, ArkDispatcher>();
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandPreProcessorMiddleware<,>));
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandPostProcessorMiddleware<,>));
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandExceptionMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryPreProcessorMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryPostProcessorMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryExceptionMiddleware<,>));

        if (options.HandlerRegistration == HandlerRegistrationMode.Generated ||
            options.HandlerRegistration == HandlerRegistrationMode.Both)
            RegisterGeneratedHandlers(services);

        if (assemblies is null || assemblies.Length == 0)
            assemblies = new[] { Assembly.GetCallingAssembly() };

        if (options.HandlerRegistration == HandlerRegistrationMode.Reflection ||
            options.HandlerRegistration == HandlerRegistrationMode.Generated ||
            options.HandlerRegistration == HandlerRegistrationMode.Both)
        {
            foreach (var assembly in assemblies)
                RegisterHandlers(services, assembly, options);
        }

        services.TryAddTransient(typeof(IEventPublisher<>), typeof(ParallelEventPublisher<>));

        return services;
    }

    /// <summary>
    /// Asynchronously registers Ark messaging dispatcher and handlers.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">Messaging configuration options.</param>
    /// <param name="assemblies">Assemblies containing CQRS handlers. When <c>null</c> or empty the calling assembly is scanned.</param>
    public static async Task<IServiceCollection> AddArkMessagingAsync(this IServiceCollection services, ArkMessagingOptions options, params Assembly[] assemblies)
    {
        // Ensure logger dependencies can be resolved in asynchronous setups
        services.AddLogging();
        services.AddTransient<IArkDispatcher, ArkDispatcher>();
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandPreProcessorMiddleware<,>));
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandPostProcessorMiddleware<,>));
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(CommandExceptionMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryPreProcessorMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryPostProcessorMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(QueryExceptionMiddleware<,>));

        if (options.HandlerRegistration == HandlerRegistrationMode.Generated ||
            options.HandlerRegistration == HandlerRegistrationMode.Both)
            RegisterGeneratedHandlers(services);

        if (assemblies is null || assemblies.Length == 0)
            assemblies = new[] { Assembly.GetCallingAssembly() };

        if (options.HandlerRegistration == HandlerRegistrationMode.Reflection ||
            options.HandlerRegistration == HandlerRegistrationMode.Generated ||
            options.HandlerRegistration == HandlerRegistrationMode.Both)
        {
            foreach (var assembly in assemblies)
                await RegisterHandlersAsync(services, assembly, options).ConfigureAwait(false);
        }

        services.TryAddTransient(typeof(IEventPublisher<>), typeof(ParallelEventPublisher<>));

        return services;
    }

    /// <summary>
    /// Scans the assembly and registers all command, query and event handlers.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    private static readonly Dictionary<Guid, List<(Type iface, Type impl)>> _reflectionMemoryCache = new();
    private static Dictionary<Guid, List<(string iface, string impl)>>? _reflectionFileCache;

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, ArkMessagingOptions options)
        => RegisterHandlersAsync(services, assembly, options).GetAwaiter().GetResult();

    private static async Task RegisterHandlersAsync(IServiceCollection services, Assembly assembly, ArkMessagingOptions options)
    {
        var mvid = assembly.ManifestModule.ModuleVersionId;
        if (options.ReflectionCache == RegistrationCacheMode.Memory && _reflectionMemoryCache.TryGetValue(mvid, out var cached))
        {
            foreach (var (iface, impl) in cached)
                services.AddTransient(iface, impl);
            return;
        }

        if (options.ReflectionCache == RegistrationCacheMode.File)
        {
            var path = options.ReflectionCacheFile ?? Path.Combine(Path.GetTempPath(), "ark_reflection_cache.json");
            if (_reflectionFileCache is null)
                _reflectionFileCache = await LoadFileCacheAsync(path).ConfigureAwait(false);

            if (_reflectionFileCache.TryGetValue(mvid, out var cachedPairs))
            {
                foreach (var pair in cachedPairs)
                {
                    var iface = Type.GetType(pair.iface);
                    var impl = Type.GetType(pair.impl);
                    if (iface is not null && impl is not null)
                        services.AddTransient(iface, impl);
                }
                return;
            }
        }

        var handlerTypes = assembly.GetTypes();
        var tmp = new List<(Type iface, Type impl)>();
        
        foreach (var type in handlerTypes)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                var definition = iface.GetGenericTypeDefinition();
                if (definition == typeof(ICommandHandler<,>) ||
                    definition == typeof(IQueryHandler<,>) ||
                    definition == typeof(IEventHandler<>) ||
                    definition == typeof(IStreamRequestHandler<,>) ||
                    definition == typeof(ICommandPreProcessor<>) ||
                    definition == typeof(ICommandPostProcessor<,>) ||
                    definition == typeof(IQueryPreProcessor<>) ||
                    definition == typeof(IQueryPostProcessor<,>))
                {
                    if (type.ContainsGenericParameters || iface.ContainsGenericParameters)
                    {
                        var serviceDef = iface.GetGenericTypeDefinition();
                        var implDef = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
                        if (serviceDef.GetGenericArguments().Length == implDef.GetGenericArguments().Length)
                        {
                            services.AddTransient(serviceDef, implDef);
                            tmp.Add((serviceDef, implDef));
                        }
                        continue;
                    }
                    else
                    {
                        services.AddTransient(iface, type);
                        tmp.Add((iface, type));
                    }
                }
            }
        }

        if (options.ReflectionCache == RegistrationCacheMode.Memory)
        {
            _reflectionMemoryCache[mvid] = tmp;
        }
        else if (options.ReflectionCache == RegistrationCacheMode.File)
        {
            var path = options.ReflectionCacheFile ?? Path.Combine(Path.GetTempPath(), "ark_reflection_cache.json");
            if (_reflectionFileCache is null)
                _reflectionFileCache = new();
            _reflectionFileCache[mvid] = tmp.Select(t => (t.iface.AssemblyQualifiedName!, t.impl.AssemblyQualifiedName!)).ToList();
            await SaveFileCacheAsync(path, _reflectionFileCache).ConfigureAwait(false);
        }
    }

    private static async Task<Dictionary<Guid, List<(string iface, string impl)>>> LoadFileCacheAsync(string path)
    {
        if (!File.Exists(path))
            return new();
        try
        {
            await using var stream = File.OpenRead(path);
            var dict = await System.Text.Json.JsonSerializer.DeserializeAsync<Dictionary<Guid, List<(string iface, string impl)>>>(stream).ConfigureAwait(false);
            return dict ?? new();
        }
        catch
        {
            return new();
        }
    }

    private static async Task SaveFileCacheAsync(string path, Dictionary<Guid, List<(string iface, string impl)>> cache)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using var stream = File.Create(path);
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, cache).ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Registers a command middleware. Open generic middlewares are supported.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCommandMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class
    {
        if (typeof(TMiddleware).IsGenericTypeDefinition || typeof(TMiddleware).ContainsGenericParameters)
        {
            services.AddTransient(typeof(ICommandMiddleware<,>), typeof(TMiddleware));
        }
        else
        {
            var iface = typeof(TMiddleware).GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandMiddleware<,>));
            if (iface is null)
                throw new ArgumentException($"{typeof(TMiddleware)} does not implement ICommandMiddleware<TCommand, TResult>");
            services.AddTransient(iface, typeof(TMiddleware));
        }
        return services;
    }

    /// <summary>
    /// Registers a command middleware by its type at runtime.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="middlewareType">Middleware type to register.</param>
    public static IServiceCollection AddCommandMiddleware(this IServiceCollection services, Type middlewareType)
    {
        if (middlewareType.IsGenericTypeDefinition || middlewareType.ContainsGenericParameters)
        {
            services.AddTransient(typeof(ICommandMiddleware<,>), middlewareType);
        }
        else
        {
            var iface = middlewareType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandMiddleware<,>));
            if (iface is null)
                throw new ArgumentException($"{middlewareType} does not implement ICommandMiddleware<TCommand, TResult>");
            services.AddTransient(iface, middlewareType);
        }
        return services;
    }

    /// <summary>
    /// Registers a query middleware. Open generic middlewares are supported.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddQueryMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class
    {
        if (typeof(TMiddleware).IsGenericTypeDefinition || typeof(TMiddleware).ContainsGenericParameters)
        {
            services.AddTransient(typeof(IQueryMiddleware<,>), typeof(TMiddleware));
        }
        else
        {
            var iface = typeof(TMiddleware).GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryMiddleware<,>));
            if (iface is null)
                throw new ArgumentException($"{typeof(TMiddleware)} does not implement IQueryMiddleware<TQuery, TResult>");
            services.AddTransient(iface, typeof(TMiddleware));
        }
        return services;
    }

    /// <summary>
    /// Registers an event middleware. Open generic middlewares are supported.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddEventMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class
    {
        if (typeof(TMiddleware).IsGenericTypeDefinition || typeof(TMiddleware).ContainsGenericParameters)
        {
            services.AddTransient(typeof(IEventMiddleware<>), typeof(TMiddleware));
        }
        else
        {
            var iface = typeof(TMiddleware).GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventMiddleware<>));
            if (iface is null)
                throw new ArgumentException($"{typeof(TMiddleware)} does not implement IEventMiddleware<TEvent>");
            services.AddTransient(iface, typeof(TMiddleware));
        }
        return services;
    }

    /// <summary>
    /// Registers an event middleware by its type at runtime.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="middlewareType">Middleware type to register.</param>
    public static IServiceCollection AddEventMiddleware(this IServiceCollection services, Type middlewareType)
    {
        if (middlewareType.IsGenericTypeDefinition || middlewareType.ContainsGenericParameters)
        {
            services.AddTransient(typeof(IEventMiddleware<>), middlewareType);
        }
        else
        {
            var iface = middlewareType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventMiddleware<>));
            if (iface is null)
                throw new ArgumentException($"{middlewareType} does not implement IEventMiddleware<TEvent>");
            services.AddTransient(iface, middlewareType);
        }
        return services;
    }

    /// <summary>
    /// Registers a stream middleware. Open generic middlewares are supported.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    public static IServiceCollection AddStreamMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class
    {
        if (typeof(TMiddleware).IsGenericTypeDefinition || typeof(TMiddleware).ContainsGenericParameters)
        {
            services.AddTransient(typeof(IStreamMiddleware<,>), typeof(TMiddleware));
        }
        else
        {
            var iface = typeof(TMiddleware).GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamMiddleware<,>));
            if (iface is null)
                throw new ArgumentException($"{typeof(TMiddleware)} does not implement IStreamMiddleware<TRequest,T>");
            services.AddTransient(iface, typeof(TMiddleware));
        }
        return services;
    }

    /// <summary>
    /// Registers default logging middlewares for commands, queries and events.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCqrsLogging(this IServiceCollection services)
    {
        // Provide a default logging configuration if none has been registered
        services.AddLogging();
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(LoggingCommandMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(LoggingQueryMiddleware<,>));
        services.AddTransient(typeof(IEventMiddleware<>), typeof(LoggingEventMiddleware<>));
        return services;
    }

    /// <summary>
    /// Registers AI decision middleware for commands.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The provided <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddAiCommandMiddleware(this IServiceCollection services)
    {
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(AiCommandMiddleware<,>));
        return services;
    }

    /// <summary>
    /// Registers <see cref="RetryCommandMiddleware{TCommand, TResult}"/> for all commands.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional action to configure <see cref="RetryOptions"/>.</param>
    public static IServiceCollection AddRetryCommandMiddleware(this IServiceCollection services, Action<RetryOptions>? configure = null)
    {
        var opts = new RetryOptions();
        configure?.Invoke(opts);
        services.AddSingleton(opts);
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(RetryCommandMiddleware<,>));
        return services;
    }
    /// <summary>
    /// Registers Polly-based resilience middlewares for commands, queries and events.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="policy">Polly policy applied to handlers.</param>
    public static IServiceCollection AddPollyResilience(this IServiceCollection services, IAsyncPolicy policy)
    {
        services.AddSingleton(policy);
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(PollyCommandMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(PollyQueryMiddleware<,>));
        services.AddTransient(typeof(IEventMiddleware<>), typeof(PollyEventMiddleware<>));
        return services;
    }

    /// <summary>
    /// Registers Polly-based resilience middlewares using a typed policy.
    /// </summary>
    /// <typeparam name="TResult">Result type handled by the policy.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="policy">Typed Polly policy.</param>
    public static IServiceCollection AddPollyResilience<TResult>(this IServiceCollection services, IAsyncPolicy<Result<TResult>> policy)
    {
        services.AddSingleton<IAsyncPolicy<Result<TResult>>>(policy);
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(PollyCommandMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(PollyQueryMiddleware<,>));
        services.AddTransient(typeof(IEventMiddleware<>), typeof(PollyEventMiddleware<>));
        return services;
    }

    /// <summary>
    /// Registers Microsoft resilience pipeline middlewares for commands, queries and events.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static IServiceCollection AddResiliencePipeline(this IServiceCollection services)
    {
        services.AddTransient(typeof(ICommandMiddleware<,>), typeof(PipelineCommandMiddleware<,>));
        services.AddTransient(typeof(IQueryMiddleware<,>), typeof(PipelineQueryMiddleware<,>));
        services.AddTransient(typeof(IEventMiddleware<>), typeof(PipelineEventMiddleware<>));
        return services;
    }


    #endregion Methods (Public)
}

