namespace ModernizationAssessor.Shared.Models;

public sealed record AssessmentReport
{
    public required string CustomerName { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
    public required string GeneratedBy { get; init; }
    public required IReadOnlyList<Workload> Workloads { get; init; }
    public required IReadOnlyList<Classification> Classifications { get; init; }

    public object? CostModel { get; init; }
    public object? MigrationPlan { get; init; }
    public object? RiskRegister { get; init; }
}
