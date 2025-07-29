namespace Ark.Alliance.Core.Mediator.Generators;

/// <summary>
/// Represents a handler interface and implementation pair used during
/// source generation.
/// </summary>
/// <summary>
/// Model representing a handler interface and its implementation type.
/// Using <c>record class</c> ensures this is treated as a reference type which
/// satisfies the generic constraint used by <see cref="UniversalIncrementalGenerator{TInput,TOutput}"/>.
/// </summary>
public sealed record class HandlerRegistration(string Interface, string Implementation);
