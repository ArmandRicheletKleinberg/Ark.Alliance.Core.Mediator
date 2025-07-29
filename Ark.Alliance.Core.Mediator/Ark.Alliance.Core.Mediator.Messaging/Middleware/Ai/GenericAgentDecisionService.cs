//using System.Threading;
//using System.Threading.Tasks;

//namespace Ark.Alliance.Core.Mediator.Messaging;

///// <summary>
///// Basic decision service relying on <see cref="Diag"/> diagnostics from
///// <c>Ark.Core.Agent.Generic</c>.
///// </summary>
//public class GenericAgentDecisionService : ICommandDecisionService
//{
//    private readonly Diag _diag;

//    public GenericAgentDecisionService(Diag diag)
//    {
//        _diag = diag;
//    }

//    /// <inheritdoc />
//    public Task<Result> EvaluateAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
//        where TCommand : ICommand<TResult>
//    {
//        _diag.Loggers.Diagnostics($"Evaluating {typeof(TCommand).Name}");
//        return Task.FromResult(Result.Success);
//    }
//}
