namespace Ark.Alliance.Core.Mediator.Messaging;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Middleware that executes registered <see cref="IQueryExceptionAction{TQuery,TException}"/>
/// and <see cref="IQueryExceptionHandler{TQuery,TResult,TException}"/> when a query handler throws.
/// </summary>
public class QueryExceptionMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly IServiceProvider _provider;

    public QueryExceptionMiddleware(IServiceProvider provider) => _provider = provider;

    public async Task<Result<TResult>> HandleAsync(TQuery query, QueryHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ExecuteActions(query, ex, cancellationToken).ConfigureAwait(false);
            var handled = await ExecuteHandlers(query, ex, cancellationToken).ConfigureAwait(false);
            if (handled.Handled && handled.Result is not null)
                return handled.Result;
            throw;
        }
    }

    private async Task ExecuteActions(TQuery query, Exception exception, CancellationToken ct)
    {
        foreach (var type in GetExceptionTypes(exception.GetType()))
        {
            var actionInterface = typeof(IQueryExceptionAction<,>).MakeGenericType(typeof(TQuery), type);
            var actions = (IEnumerable<object>?)_provider.GetService(typeof(IEnumerable<>).MakeGenericType(actionInterface));
            if (actions is null) continue;
            var method = actionInterface.GetMethod("ExecuteAsync")!;
            foreach (var action in actions)
                await ((Task)method.Invoke(action, new object[] { query, exception, ct })!).ConfigureAwait(false);
        }
    }

    private async Task<QueryExceptionHandlerState<TResult>> ExecuteHandlers(TQuery query, Exception exception, CancellationToken ct)
    {
        var state = new QueryExceptionHandlerState<TResult>();
        foreach (var type in GetExceptionTypes(exception.GetType()))
        {
            var handlerInterface = typeof(IQueryExceptionHandler<,,>).MakeGenericType(typeof(TQuery), typeof(TResult), type);
            var handlers = (IEnumerable<object>?)_provider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerInterface));
            if (handlers is null) continue;
            var method = handlerInterface.GetMethod("HandleAsync")!;
            foreach (var handler in handlers)
            {
                await ((Task)method.Invoke(handler, new object[] { query, exception, state, ct })!).ConfigureAwait(false);
                if (state.Handled)
                    return state;
            }
        }
        return state;
    }

    private static IEnumerable<Type> GetExceptionTypes(Type? exceptionType)
    {
        while (exceptionType is not null && exceptionType != typeof(object))
        {
            yield return exceptionType;
            exceptionType = exceptionType.GetTypeInfo().BaseType;
        }
    }
}
