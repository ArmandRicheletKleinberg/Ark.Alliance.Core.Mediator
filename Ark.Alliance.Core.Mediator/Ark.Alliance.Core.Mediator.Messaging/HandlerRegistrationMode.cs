namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Determines how handlers are registered when calling <see cref="IServiceCollectionExtensions.AddArkMessaging"/>.
/// </summary>
public enum HandlerRegistrationMode
{
    /// <summary>Use only the source-generated registrations.</summary>
    Generated,

    /// <summary>Use runtime reflection to discover handlers.</summary>
    Reflection,

    /// <summary>Use both generated registrations and reflection.</summary>
    Both
}

