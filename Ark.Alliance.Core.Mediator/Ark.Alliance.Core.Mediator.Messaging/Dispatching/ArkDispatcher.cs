using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Ark.Alliance.Core.Mediator.Messaging;

using Microsoft.Extensions.Logging;
using Ark.Alliance.Core;


namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Default implementation of <see cref="IArkDispatcher"/> using <see cref="IServiceProvider"/>.
/// Resolves handlers and middlewares from the application's dependency container.
/// </summary>
public class ArkDispatcher : IArkDispatcher
{
    #region Fields

    private readonly IServiceProvider _provider;
    private readonly ILogger<ArkDispatcher> _logger;

    #endregion Fields

    /// <summary>
    /// Initializes a new instance of the <see cref="ArkDispatcher"/> class.
    /// </summary>
    /// <param name="provider">Service provider used to resolve handlers and middlewares.</param>
    #region Constructors

    public ArkDispatcher(IServiceProvider provider, ILogger<ArkDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    #endregion Constructors

    /// <inheritdoc />
    #region Methods (Public)

    public async Task<Result<TResult>> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));
        _logger.LogInformation("Dispatching command {Command}", typeof(TCommand).Name);

        var handler = _provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        var middlewares = _provider.GetServices<ICommandMiddleware<TCommand, TResult>>() ?? Enumerable.Empty<ICommandMiddleware<TCommand, TResult>>();

        CommandHandlerDelegate<TResult> handlerDelegate = () => handler.HandleAsync(command, cancellationToken);

        foreach (var middleware in middlewares.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => middleware.HandleAsync(command, next, cancellationToken);
        }

        try
        {
            var result = await handlerDelegate();
            _logger.LogInformation("Command {Command} completed with status {Status}", typeof(TCommand).Name, result.Status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Command {Command} failed", typeof(TCommand).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Result<TResult>> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));
        _logger.LogInformation("Dispatching query {Query}", typeof(TQuery).Name);

        var handler = _provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        var middlewares = _provider.GetServices<IQueryMiddleware<TQuery, TResult>>() ?? Enumerable.Empty<IQueryMiddleware<TQuery, TResult>>();

        QueryHandlerDelegate<TResult> handlerDelegate = () => handler.HandleAsync(query, cancellationToken);

        foreach (var middleware in middlewares.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => middleware.HandleAsync(query, next, cancellationToken);
        }

        try
        {
            var result = await handlerDelegate();
            _logger.LogInformation("Query {Query} completed with status {Status}", typeof(TQuery).Name, result.Status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query {Query} failed", typeof(TQuery).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Result<object?>> SendAsync(object command, CancellationToken cancellationToken = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var iface = command.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
        if (iface is null)
            throw new ArgumentException($"{command.GetType()} does not implement ICommand<T>", nameof(command));

        var method = typeof(ArkDispatcher)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Single(m => m.Name == nameof(SendAsync) && m.IsGenericMethodDefinition);
        method = method.MakeGenericMethod(command.GetType(), iface.GenericTypeArguments[0]);
        var task = (Task)method.Invoke(this, new object[] { command, cancellationToken })!;
        await task.ConfigureAwait(false);
        var result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        return ConvertResult(result);
    }

    /// <inheritdoc />
    public async Task<Result<object?>> QueryAsync(object query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        var iface = query.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        if (iface is null)
            throw new ArgumentException($"{query.GetType()} does not implement IQuery<T>", nameof(query));

        var method = typeof(ArkDispatcher)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Single(m => m.Name == nameof(QueryAsync) && m.IsGenericMethodDefinition);
        method = method.MakeGenericMethod(query.GetType(), iface.GenericTypeArguments[0]);
        var task = (Task)method.Invoke(this, new object[] { query, cancellationToken })!;
        await task.ConfigureAwait(false);
        var result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        return ConvertResult(result);
    }

    /// <inheritdoc />
    public Task PublishAsync(object @event, CancellationToken cancellationToken = default)
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var iface = @event.GetType().GetInterfaces()
            .FirstOrDefault(i => i == typeof(IEvent));
        if (iface is null)
            throw new ArgumentException($"{@event.GetType()} does not implement IEvent", nameof(@event));

        var method = typeof(ArkDispatcher)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Single(m => m.Name == nameof(PublishAsync) && m.IsGenericMethodDefinition);
        method = method.MakeGenericMethod(@event.GetType());
        return (Task)method.Invoke(this, new object[] { @event, cancellationToken })!;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<object?> CreateStream(object query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));

        var iface = query.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>));
        if (iface is null)
            throw new ArgumentException($"{query.GetType()} does not implement IStreamRequest<T>", nameof(query));

        var method = typeof(ArkDispatcher)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Single(m => m.Name == nameof(CreateStream) && m.IsGenericMethodDefinition);
        method = method.MakeGenericMethod(iface.GenericTypeArguments[0]);
        var result = (IAsyncEnumerable<object?>)method.Invoke(this, new object[] { query, cancellationToken })!;
        return result;
    }

    private static Result<object?> ConvertResult(object result)
    {
        var baseResult = (Result)result;
        var data = result.GetType().GetProperty("Data")?.GetValue(result);
        var converted = new Result<object?>(baseResult.Status, (object?)data, baseResult.Exception);
        typeof(Result).GetProperty("Reason")!.SetValue(converted, baseResult.Reason);
        return converted;
    }

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));
        _logger.LogInformation("Publishing event {Event}", typeof(TEvent).Name);

        var handlers = _provider.GetServices<IEventHandler<TEvent>>() ?? Enumerable.Empty<IEventHandler<TEvent>>();
        var middlewares = _provider.GetServices<IEventMiddleware<TEvent>>() ?? Enumerable.Empty<IEventMiddleware<TEvent>>();

        var publisher = _provider.GetService<IEventPublisher<TEvent>>() ?? new ParallelEventPublisher<TEvent>();

        EventHandlerDelegate handlerDelegate = () => publisher.PublishAsync(handlers, @event, cancellationToken);

        foreach (var middleware in middlewares.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => middleware.HandleAsync(@event, next, cancellationToken);
        }

        try
        {
            await handlerDelegate();
            _logger.LogInformation("Event {Event} published", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event {Event} failed", typeof(TEvent).Name);
            throw;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<T> CreateStream<T>(IStreamRequest<T> query, CancellationToken cancellationToken = default)
    {
        if (query is null)
            throw new ArgumentNullException(nameof(query));
        _logger.LogInformation("Creating stream for {Query}", query.GetType().Name);

        var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(query.GetType(), typeof(T));
        dynamic handler = _provider.GetRequiredService(handlerType);

        var middlewareType = typeof(IStreamMiddleware<,>).MakeGenericType(query.GetType(), typeof(T));
        var middlewares = _provider.GetServices(middlewareType)?.Cast<dynamic>() ?? Enumerable.Empty<dynamic>();

        StreamHandlerDelegate<T> handlerDelegate = () => handler.HandleAsync((dynamic)query, cancellationToken);

        foreach (var middleware in middlewares.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => middleware.HandleAsync((dynamic)query, next, cancellationToken);
        }

        return handlerDelegate();
    }

    #endregion Methods (Public)
}
