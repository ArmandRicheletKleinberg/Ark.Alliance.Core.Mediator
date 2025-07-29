using System;
using System.Collections.Generic;

namespace Ark.Alliance.Core;

/// <summary>
/// Provides helpers for <see cref="IEnumerable{T}"/> sequences.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Executes an action for each element of the sequence.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="action">The action to execute.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source is null)
            return;

        foreach (var item in source)
            action(item);
    }
}
