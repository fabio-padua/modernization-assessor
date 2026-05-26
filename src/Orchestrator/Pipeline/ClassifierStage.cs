using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

internal sealed class ClassifierStage : IClassifierStage
{
    public Task<IReadOnlyList<Classification>> ClassifyAsync(
        IReadOnlyList<Workload> workloads,
        CancellationToken ct)
        => Task.FromResult(Classify(workloads));

    public IReadOnlyList<Classification> Classify(IEnumerable<Workload> workloads)
    {
        var results = new List<Classification>();
        foreach (var w in workloads) results.Add(ClassifyOne(w));
        return results;
    }

    private static Classification ClassifyOne(Workload w)
    {
        var os = w.OperatingSystem?.ToLowerInvariant() ?? "";
        var app = w.ApplicationName.ToLowerInvariant();
        var db = w.DatabaseEngine?.ToLowerInvariant() ?? "";

        if (os.Contains("zos") || os.Contains("mainframe"))
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Retain,
                TargetAzureService = "On-prem / Mainframe modernization (out of scope v0.1)",
                Confidence = 0.95,
                Rationale = "Mainframe workloads require a dedicated modernization track.",
                Risks = new[] { "Skills scarcity", "Regulatory dependencies", "Vendor lock-in" }
            };
        }

        if (app.Contains("sharepoint") && (w.Criticality ?? BusinessCriticality.Low) <= BusinessCriticality.Medium)
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Replace,
                TargetAzureService = "Microsoft 365 / SharePoint Online",
                Confidence = 0.85,
                Rationale = "On-prem SharePoint with non-critical usage is a strong candidate for SaaS replacement.",
                Prerequisites = new[] { "Content migration assessment", "Identity sync via Entra Connect" }
            };
        }

        if (app.Contains("dev") || app.Contains("test") || app.Contains("sandbox") || app.Contains("jenkins"))
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Retire,
                TargetAzureService = "GitHub Actions / Azure DevOps (managed CI)",
                Confidence = 0.7,
                Rationale = "Low-criticality dev/test infrastructure should be replaced with managed CI and ephemeral dev environments.",
                Risks = new[] { "Build pipeline migration effort" }
            };
        }

        if (db.Contains("sqlserver"))
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Refactor,
                TargetAzureService = "Azure SQL Managed Instance",
                Confidence = 0.8,
                Rationale = "SQL Server workload — Azure SQL MI offers near-100% feature parity with minimal refactor.",
                Prerequisites = new[] { "DMA assessment", "Connection-string update", "Cross-region DR planning" }
            };
        }

        if (db.Contains("oracle"))
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Refactor,
                TargetAzureService = "Oracle Database@Azure or Azure DB for PostgreSQL",
                Confidence = 0.6,
                Rationale = "Oracle workloads need deeper assessment; default is Oracle@Azure for lift, PostgreSQL for refactor.",
                Risks = new[] { "License entitlement review", "PL/SQL surface area" }
            };
        }

        if ((w.PublicFacing ?? false) && os.Contains("ubuntu"))
        {
            return new Classification
            {
                WorkloadId = w.Id,
                Strategy = ModernizationStrategy.Replatform,
                TargetAzureService = "Azure Container Apps",
                Confidence = 0.82,
                Rationale = "Public-facing Linux HTTP workload — Container Apps provides autoscale, KEDA, managed ingress.",
                Prerequisites = new[] { "Containerization (Dockerfile)", "Externalize config to App Configuration + Key Vault" }
            };
        }

        return new Classification
        {
            WorkloadId = w.Id,
            Strategy = ModernizationStrategy.Rehost,
            TargetAzureService = "Azure VM (matched SKU)",
            Confidence = 0.6,
            Rationale = "No strong PaaS signal — default to IaaS lift with right-sizing during migration.",
            Prerequisites = new[] { "Azure Migrate assessment", "Right-sizing review" }
        };
    }
}
