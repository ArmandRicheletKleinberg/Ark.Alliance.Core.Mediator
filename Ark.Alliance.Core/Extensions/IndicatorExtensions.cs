using Ark.Alliance.Core.Diagnostics;

namespace Ark.Alliance.Core;

/// <summary>
/// Helper extensions for working with indicator dictionaries.
/// </summary>
public static class IndicatorExtensions
{
    /// <summary>
    /// Sets the value of the indicator with the specified key if present.
    /// </summary>
    /// <typeparam name="T">Type of the indicator value.</typeparam>
    /// <param name="indicators">Dictionary of indicators.</param>
    /// <param name="key">Indicator key.</param>
    /// <param name="value">Value to set.</param>
    public static void SetIndicatorValue<T>(this IDictionary<string, Indicator>? indicators, string key, T value)
    {
        if (indicators?.GetValue(key) is Indicator<T> indicator)
            indicator.SetValue(value);
    }
}
