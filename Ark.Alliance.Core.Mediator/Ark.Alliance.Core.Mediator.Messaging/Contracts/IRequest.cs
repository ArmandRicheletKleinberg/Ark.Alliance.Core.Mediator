namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Base request carrying a response of type <typeparamref name="TResponse"/>.
/// Matches the pattern of MediatR's <c>IRequest&lt;TResponse&gt;</c>.
/// </summary>
/// <typeparam name="TResponse">Type returned when the request is handled.</typeparam>
public interface IRequest<out TResponse> { }

/// <summary>
/// Marker request that does not return a specific value.
/// Equivalent to <c>IRequest&lt;Unit&gt;</c> in MediatR.
/// </summary>
public interface IRequest { }

