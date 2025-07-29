namespace Ark.Alliance.Core.Mediator.Generators;

/// <summary>
/// Defines caching strategies for <see cref="HandlerRegistrationGenerator"/>.
/// </summary>
public enum RegistrationCacheMode
{
    #region Values

    /// <summary>
    /// No caching. The generator always emits source.
    /// </summary>
    None,

    /// <summary>
    /// Cache hashes in memory during the current compilation.
    /// </summary>
    Memory,

    /// <summary>
    /// Persist cache to a file to reuse across compilations.
    /// </summary>
    File,

    /// <summary>
    /// Combines memory and file caching. Entries are stored in memory for the
    /// current compilation and persisted to disk to be reused on subsequent
    /// builds.
    /// </summary>
    Hybrid

    #endregion Values
}

