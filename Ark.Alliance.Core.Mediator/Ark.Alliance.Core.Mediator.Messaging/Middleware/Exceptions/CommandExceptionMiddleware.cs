namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware that executes registered <see cref="ICommandExceptionAction{TCommand,TException}"/>
/// and <see cref="ICommandExceptionHandler{TCommand,TResult,TException}"/> when a command handler throws.
/// </summary>
public class CommandExceptionMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IServiceProvider _provider;

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the middleware.
    /// </summary>
    /// <param name="provider">Service provider used to resolve handlers.</param>
    public CommandExceptionMiddleware(IServiceProvider provider) => _provider = provider;

    #endregion Constructors

    #region Methods (Public)

    /// <summary>
    /// Executes the command and handles any thrown exceptions using registered handlers.
    /// </summary>
    /// <param name="command">Command being executed.</param>
    /// <param name="next">Delegate that invokes the command handler.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The result produced by the handler or an alternative from exception handlers.</returns>
    public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ExecuteActions(command, ex, cancellationToken).ConfigureAwait(false);
            var handled = await ExecuteHandlers(command, ex, cancellationToken).ConfigureAwait(false);
            if (handled.Handled && handled.Result is not null)
                return handled.Result;
            throw;
        }
    }

    #endregion Methods (Public)

    #region Methods (Private)

    /// <summary>
    /// Executes all registered exception actions for the provided command.
    /// </summary>
    private async Task ExecuteActions(TCommand command, Exception exception, CancellationToken ct)
    {
        foreach (var type in GetExceptionTypes(exception.GetType()))
        {
            var actionInterface = typeof(ICommandExceptionAction<,>).MakeGenericType(typeof(TCommand), type);
            var actions = (IEnumerable<object>?)_provider.GetService(typeof(IEnumerable<>).MakeGenericType(actionInterface));
            if (actions is null) continue;
            var method = actionInterface.GetMethod("ExecuteAsync")!;
            foreach (var action in actions)
                await ((Task)method.Invoke(action, new object[] { command, exception, ct })!).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes registered exception handlers and returns their state.
    /// </summary>
    private async Task<CommandExceptionHandlerState<TResult>> ExecuteHandlers(TCommand command, Exception exception, CancellationToken ct)
    {
        var state = new CommandExceptionHandlerState<TResult>();
        foreach (var type in GetExceptionTypes(exception.GetType()))
        {
            var handlerInterface = typeof(ICommandExceptionHandler<,,>).MakeGenericType(typeof(TCommand), typeof(TResult), type);
            var handlers = (IEnumerable<object>?)_provider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerInterface));
            if (handlers is null) continue;
            var method = handlerInterface.GetMethod("HandleAsync")!;
            foreach (var handler in handlers)
            {
                await ((Task)method.Invoke(handler, new object[] { command, exception, state, ct })!).ConfigureAwait(false);
                if (state.Handled)
                    return state;
            }
        }
        return state;
    }

    /// <summary>
    /// Enumerates the provided exception type and its base types.
    /// </summary>
    private static IEnumerable<Type> GetExceptionTypes(Type? exceptionType)
    {
        while (exceptionType is not null && exceptionType != typeof(object))
        {
            yield return exceptionType;
            exceptionType = exceptionType.GetTypeInfo().BaseType;
        }
    }

    #endregion Methods (Private)
}
