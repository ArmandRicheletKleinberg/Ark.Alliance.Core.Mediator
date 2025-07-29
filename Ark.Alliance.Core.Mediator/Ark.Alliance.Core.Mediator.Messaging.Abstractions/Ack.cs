using System.Diagnostics.CodeAnalysis;

namespace Ark.Alliance.Core.Mediator.Messaging.Abstractions;

/// <summary>
/// Defines publication acknowledgement results.
/// </summary>
public enum Ack
{
    #region Values
    /// <summary>Publication succeeded.</summary>
    Ok,

    /// <summary>Publication failed.</summary>
    Fail
    #endregion Values
}
