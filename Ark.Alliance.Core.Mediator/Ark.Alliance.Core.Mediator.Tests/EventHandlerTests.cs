using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ark.Alliance.Core.Mediator.Messaging;
using Xunit;

/// <summary>
/// Tests custom <see cref="EventHandler{TEvent}"/> implementations.
/// </summary>
public class EventHandlerTests
{
    #region Records
    /// <summary>
    /// Simple event used to verify base <see cref="EventHandler{TEvent}"/> behavior.
    /// </summary>
    private record Ping(string Message) : IEvent;

    #endregion Records

    #region Handlers
    /// <summary>
    /// Handler writing the received message with an appended " Pong".
    /// </summary>
    private class PongChildHandler : Ark.Alliance.Core.Mediator.Messaging.EventHandler<Ping>
    {
        private readonly TextWriter _writer;
        public PongChildHandler(TextWriter writer) => _writer = writer;
        protected override void Handle(Ping @event) => _writer.WriteLine(@event.Message + " Pong");
    }

    #endregion Handlers

    #region Methods (Tests)
    /// <summary>
    /// Ensures the overridden <see cref="EventHandler{TEvent}.Handle"/> method is executed by <see cref="EventHandler{TEvent}.HandleAsync"/>.
    /// </summary>
    [Fact]
    public async Task Base_handle_method_is_invoked()
    {
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);
        IEventHandler<Ping> handler = new PongChildHandler(writer);

        await handler.HandleAsync(new Ping("Ping"), CancellationToken.None);

        var result = builder.ToString();
        Assert.Contains("Ping Pong", result);
    }
    #endregion Methods (Tests)
}
