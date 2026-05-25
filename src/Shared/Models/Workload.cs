namespace ModernizationAssessor.Shared.Models;

/// <summary>
/// Canonical normalized representation of one workload (server / application / database)
/// from a customer's IT estate. The Discovery agent emits items in this shape from raw
/// Azure Migrate / CMDB / Excel inputs.
/// </summary>
public sealed record Workload
{
    public required string Id { get; init; }
    public required string ApplicationName { get; init; }
    public string? Environment { get; init; }
    public string? OperatingSystem { get; init; }
    public int? VCpus { get; init; }
    public double? MemoryGb { get; init; }
    public double? StorageGb { get; init; }
    public string? DatabaseEngine { get; init; }
    public long? MonthlyRequests { get; init; }
    public bool? PublicFacing { get; init; }
    public BusinessCriticality? Criticality { get; init; }
    public decimal? CurrentMonthlyCostUsd { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, string> RawAttributes { get; init; }
        = new Dictionary<string, string>();
}

public enum BusinessCriticality
{
    Low,
    Medium,
    High,
    Critical
}
