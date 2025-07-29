using BenchmarkDotNet.Reports;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Ark.Alliance.Core.Mediator.Benchmarks;

internal static class BenchmarkReportGenerator
{
    private const string DefaultReport = "Benchmarks_Report.md";

    public static void AppendReport(Summary[] summaries, string? path = null)
    {
        if (summaries == null || summaries.Length == 0)
            return;

        var file = path ?? DefaultReport;
        var section = BuildSection(summaries);

        if (File.Exists(file))
        {
            File.AppendAllText(file, section);
        }
        else
        {
            var header = "# Benchmark Report" + Environment.NewLine + Environment.NewLine;
            File.WriteAllText(file, header + section);
        }
    }

    private static string BuildSection(Summary[] summaries)
    {
        var sb = new StringBuilder();
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var branch = GetCurrentBranch();
        sb.AppendLine($"## {date} - {branch}");
        sb.AppendLine();

        foreach (var summary in summaries)
        {
            sb.AppendLine($"### {summary.Title}");
            sb.AppendLine();
            sb.AppendLine("| Method | Mean (ns) | Error (ns) | StdDev (ns) |");
            sb.AppendLine("|-------|----------|-----------|------------|");
            foreach (var report in summary.Reports)
            {
                var stats = report.ResultStatistics;
                if (stats == null)
                    continue;
                var name = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;
                sb.AppendLine($"| {name} | {stats.Mean:F2} | {stats.StandardError:F2} | {stats.StandardDeviation:F2} |");
            }
            sb.AppendLine();

            if (summary.Title.Contains("Comparison"))
            {
                AppendComparison(summary, sb);
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private static void AppendComparison(Summary summary, StringBuilder sb)
    {
        var medSend = GetMean(summary, "MediatR_Send");
        var arkSend = GetMean(summary, "Ark_Send");
        var medPub = GetMean(summary, "MediatR_Publish");
        var arkPub = GetMean(summary, "Ark_Publish");

        var hasSend = !double.IsNaN(medSend) && !double.IsNaN(arkSend);
        var hasPublish = !double.IsNaN(medPub) && !double.IsNaN(arkPub);

        if (hasSend || hasPublish)
        {
            sb.AppendLine("| Scenario | MediatR (ns) | Ark (ns) | Ark vs MediatR |");
            sb.AppendLine("|---------|-------------|---------|---------------|");

            if (hasSend)
            {
                var rate = arkSend / medSend * 100.0;
                sb.AppendLine($"| Send | {medSend:F2} | {arkSend:F2} | {rate:F2}% |");
            }

            if (hasPublish)
            {
                var rate = arkPub / medPub * 100.0;
                sb.AppendLine($"| Publish | {medPub:F2} | {arkPub:F2} | {rate:F2}% |");
            }

            sb.AppendLine();
        }
    }

    private static double GetMean(Summary summary, string method)
    {
        var report = summary.Reports.FirstOrDefault(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == method);
        return report?.ResultStatistics?.Mean ?? double.NaN;
    }

    private static string GetCurrentBranch()
    {
        try
        {
            var psi = new ProcessStartInfo("git", "rev-parse --abbrev-ref HEAD")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc.WaitForExit();
            return proc.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            return "unknown";
        }
    }
}
