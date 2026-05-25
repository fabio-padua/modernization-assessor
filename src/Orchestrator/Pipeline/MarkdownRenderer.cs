using System.Text;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

internal static class MarkdownRenderer
{
    public static string Render(AssessmentReport r)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Modernization Assessment — {r.CustomerName}");
        sb.AppendLine();
        sb.AppendLine($"_Generated {r.GeneratedAt:yyyy-MM-dd HH:mm} UTC by {r.GeneratedBy}_");
        sb.AppendLine();

        sb.AppendLine("## Executive summary");
        sb.AppendLine();
        var byStrategy = r.Classifications.GroupBy(c => c.Strategy).OrderByDescending(g => g.Count());
        sb.AppendLine("| Strategy | Workloads | % of estate |");
        sb.AppendLine("|---|---:|---:|");
        var total = r.Classifications.Count;
        foreach (var g in byStrategy)
        {
            var pct = total == 0 ? 0 : (g.Count() * 100.0 / total);
            sb.AppendLine($"| {g.Key} | {g.Count()} | {pct:F0}% |");
        }
        sb.AppendLine();

        sb.AppendLine("## Per-workload recommendations");
        sb.AppendLine();
        sb.AppendLine("| Workload | OS / DB | Strategy | Target service | Confidence |");
        sb.AppendLine("|---|---|---|---|---:|");
        foreach (var w in r.Workloads)
        {
            var c = r.Classifications.First(x => x.WorkloadId == w.Id);
            var osdb = string.Join(" / ", new[] { w.OperatingSystem, w.DatabaseEngine }.Where(s => !string.IsNullOrWhiteSpace(s)));
            sb.AppendLine($"| {w.ApplicationName} | {osdb} | {c.Strategy} | {c.TargetAzureService} | {c.Confidence:P0} |");
        }
        sb.AppendLine();

        sb.AppendLine("## Rationale detail");
        sb.AppendLine();
        foreach (var c in r.Classifications)
        {
            var w = r.Workloads.First(x => x.Id == c.WorkloadId);
            sb.AppendLine($"### {w.ApplicationName} → {c.Strategy}");
            sb.AppendLine();
            sb.AppendLine(c.Rationale);
            sb.AppendLine();
            if (c.Prerequisites.Count > 0)
            {
                sb.AppendLine("**Prerequisites**");
                foreach (var p in c.Prerequisites) sb.AppendLine($"- {p}");
                sb.AppendLine();
            }
            if (c.Risks.Count > 0)
            {
                sb.AppendLine("**Risks**");
                foreach (var r2 in c.Risks) sb.AppendLine($"- {r2}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("_v0.1 output. Cost model, migration plan, and risk register added in v0.2 once specialist agents are wired to Foundry._");
        return sb.ToString();
    }
}
