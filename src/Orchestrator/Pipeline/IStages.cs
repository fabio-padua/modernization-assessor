using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

/// <summary>
/// Discovery stage abstraction. Implemented by the deterministic
/// CsvHelper-based <see cref="DiscoveryStage"/> and by the
/// Foundry-agent-backed Discovery stage.
/// </summary>
internal interface IDiscoveryStage
{
    Task<IReadOnlyList<Workload>> NormalizeAsync(string inventoryPath, CancellationToken ct);
}

/// <summary>
/// Classifier stage abstraction. Implemented by the deterministic rule-based
/// <see cref="ClassifierStage"/> and by the Foundry-agent-backed Classifier stage.
/// </summary>
internal interface IClassifierStage
{
    Task<IReadOnlyList<Classification>> ClassifyAsync(
        IReadOnlyList<Workload> workloads,
        CancellationToken ct);
}
