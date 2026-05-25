namespace ModernizationAssessor.Shared.Models;

public sealed record Classification
{
    public required string WorkloadId { get; init; }
    public required ModernizationStrategy Strategy { get; init; }
    public required string TargetAzureService { get; init; }
    public required double Confidence { get; init; }
    public required string Rationale { get; init; }
    public IReadOnlyList<string> Risks { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites { get; init; } = Array.Empty<string>();
}

public enum ModernizationStrategy
{
    Rehost,
    Replatform,
    Refactor,
    Rebuild,
    Replace,
    Retire,
    Retain
}
