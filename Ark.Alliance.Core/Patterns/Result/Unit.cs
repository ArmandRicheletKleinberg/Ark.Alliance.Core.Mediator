namespace Ark.Alliance.Core;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents a void result. Equivalent to MediatR's <c>Unit</c> type.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    /// <summary>
    /// Single shared instance.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// A completed task returning <see cref="Value"/>.
    /// </summary>
    public static Task<Unit> Task => System.Threading.Tasks.Task.FromResult(Value);

    /// <inheritdoc />
    public override string ToString() => "()";

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public int CompareTo(Unit other) => 0;

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj) => 0;

    /// <summary>
    /// Always returns true, units are equal.
    /// </summary>
    public static bool operator ==(Unit first, Unit second) => true;

    /// <summary>
    /// Always returns false, units are equal.
    /// </summary>
    public static bool operator !=(Unit first, Unit second) => false;
}
