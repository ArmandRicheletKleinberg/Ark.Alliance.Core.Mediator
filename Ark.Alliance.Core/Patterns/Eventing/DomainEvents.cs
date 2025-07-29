using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Eventing;

/// <summary>
/// Simple domain events dispatcher for in-process notifications.
/// </summary>
public static class DomainEvents
{
    private static readonly List<Func<INotification, Task>> _handlers = new();

    /// <summary>
    /// Registers a handler for notifications of type <typeparamref name="T"/>.
    /// </summary>
    public static void Register<T>(Func<T, Task> handler) where T : INotification
    {
        _handlers.Add(n => n is T t ? handler(t) : Task.CompletedTask);
    }

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    public static Task Publish(INotification notification)
    {
        var tasks = new List<Task>(_handlers.Count);
        foreach (var handler in _handlers)
            tasks.Add(handler(notification));
        return Task.WhenAll(tasks);
    }
}
