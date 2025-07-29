namespace Ark.Alliance.Core;

/// <summary>
/// Provides helper extensions for <see cref="IDictionary{TKey,TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets a value from the dictionary if present, otherwise returns <c>null</c>.
    /// </summary>
    /// <param name="dictionary">The dictionary instance.</param>
    /// <param name="key">The key to search.</param>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <returns>The value associated with the key or <c>null</c>.</returns>
    public static TValue? GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary != null && dictionary.TryGetValue(key, out var value) ? value : default;
    }

    /// <summary>
    /// Converts a dictionary of objects to a dictionary of strings. Values are
    /// converted using <c>ToString()</c> and <c>null</c> values become empty strings.
    /// </summary>
    /// <param name="headers">The source dictionary.</param>
    /// <returns>A new dictionary with string values.</returns>
    public static IDictionary<string, string> ToStringDictionary(this IDictionary<string, object>? headers)
    {
        var result = new Dictionary<string, string>();
        if (headers is null)
            return result;

        foreach (var (key, value) in headers)
            result[key] = value?.ToString() ?? string.Empty;

        return result;
    }
}
