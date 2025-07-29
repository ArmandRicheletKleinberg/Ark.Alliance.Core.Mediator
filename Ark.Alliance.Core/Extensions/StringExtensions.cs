namespace Ark.Alliance.Core;

/// <summary>
/// Provides helper extensions for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether a string is null or empty.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns><c>true</c> if the string is null or empty.</returns>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}
