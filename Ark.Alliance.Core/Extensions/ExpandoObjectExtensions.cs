using System.Dynamic;

namespace Ark.Alliance.Core;

/// <summary>
/// Provides helper extensions for <see cref="ExpandoObject"/>.
/// </summary>
public static class ExpandoObjectExtensions
{
    /// <summary>
    /// Adds or updates a value in the <see cref="ExpandoObject"/>.
    /// </summary>
    /// <param name="expando">The target expando object.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">The value to set.</param>
    public static void AddOrUpdate(this ExpandoObject expando, string key, object? value)
    {
        var dict = (IDictionary<string, object?>)expando;
        dict[key] = value;
    }
}
