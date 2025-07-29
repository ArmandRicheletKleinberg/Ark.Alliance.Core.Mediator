namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Specifies the caching strategy used when registering handlers via reflection.
/// Matches the enum used by the source generator.
/// </summary>
public enum RegistrationCacheMode
{
    /// <summary>No caching. Handlers are discovered on each call.</summary>
    None,

    /// <summary>Cache registrations in memory for the current process.</summary>
    Memory,

    /// <summary>Persist registrations to a file for reuse across runs.</summary>
    File
}
