namespace Ark.Alliance.Core;

/// <summary>
/// Provides helper extensions for <see cref="Type"/> instances.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Creates an instance of the specified <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="type">The type to instantiate.</param>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    public static T New<T>(this Type type) where T : notnull
    {
        return (T)Activator.CreateInstance(type)!;
    }
}
