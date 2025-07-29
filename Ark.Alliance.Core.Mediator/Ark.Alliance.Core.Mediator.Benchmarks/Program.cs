using BenchmarkDotNet.Running;
using System.Linq;
namespace Ark.Alliance.Core.Mediator.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summaries = BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);

        BenchmarkReportGenerator.AppendReport(summaries.ToArray());
    }
}
