using Ark.Alliance.Core.Mediator.Messaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

public class GenericCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly TextWriter _writer;

    public GenericCommandMiddleware(TextWriter writer) => _writer = writer;

    public async Task<Result<TResult>> HandleAsync(TCommand command, CommandHandlerDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync("-- Handling Command");
        var result = await next();
        await _writer.WriteLineAsync("-- Finished Command");
        return result;
    }
}
