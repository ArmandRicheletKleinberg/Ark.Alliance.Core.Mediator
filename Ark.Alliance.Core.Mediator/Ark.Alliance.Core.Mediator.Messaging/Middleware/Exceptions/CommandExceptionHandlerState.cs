namespace Ark.Alliance.Core.Mediator.Messaging;

/// <summary>
/// Represents the result of handling an exception thrown by a command handler.
/// </summary>
/// <typeparam name="TResult">Command result type.</typeparam>
public sealed class CommandExceptionHandlerState<TResult>
{
    /// <summary>
    /// Indicates whether the exception was handled.
    /// </summary>
    public bool Handled { get; private set; }

    /// <summary>
    /// The result returned when <see cref="Handled"/> is <c>true</c>.
    /// </summary>
    public Result<TResult>? Result { get; private set; }

    /// <summary>
    /// Marks the exception as handled with the specified result.
    /// </summary>
    /// <param name="result">Result to return.</param>
    public void SetHandled(Result<TResult> result)
    {
        Handled = true;
        Result = result;
    }
}
