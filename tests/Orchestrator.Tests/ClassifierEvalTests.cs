using System.Text.Json;
using System.Text.Json.Serialization;
using ModernizationAssessor.Orchestrator.Pipeline;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Tests;

/// <summary>
/// Regression gates for the classifier: runs the deterministic <see cref="ClassifierStage"/>
/// against a labeled golden dataset and asserts strategy accuracy, target-service hints, and
/// confidence range. Foundry-mode groundedness checks are planned for v0.2.x once the agent
/// classifier is wired into the test runner.
/// </summary>
public sealed class ClassifierEvalTests
{
    private const double StrategyAccuracyThreshold = 0.80;
    private const double TargetServiceContainsThreshold = 0.80;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void Deterministic_GoldenStrategyAccuracy_AtLeastThreshold()
    {
        var cases = LoadGolden();
        Assert.NotEmpty(cases);

        var stage = new ClassifierStage();
        var workloads = cases.Select(c => c.ToWorkload()).ToList();
        var classifications = stage.Classify(workloads);

        var hits = 0;
        var misses = new List<string>();
        for (var i = 0; i < cases.Count; i++)
        {
            var expected = cases[i].ExpectedStrategy;
            var actual = classifications[i].Strategy;
            if (expected == actual)
                hits++;
            else
                misses.Add($"  {cases[i].Id} ({cases[i].ApplicationName}): expected={expected} actual={actual}");
        }

        var accuracy = (double)hits / cases.Count;
        Assert.True(
            accuracy >= StrategyAccuracyThreshold,
            $"Strategy accuracy {accuracy:P0} below threshold {StrategyAccuracyThreshold:P0}. Misses:\n{string.Join("\n", misses)}");
    }

    [Fact]
    public void Deterministic_GoldenTargetServiceContains_AtLeastThreshold()
    {
        var cases = LoadGolden();
        var stage = new ClassifierStage();
        var workloads = cases.Select(c => c.ToWorkload()).ToList();
        var classifications = stage.Classify(workloads);

        var hits = 0;
        var misses = new List<string>();
        for (var i = 0; i < cases.Count; i++)
        {
            var needle = cases[i].ExpectedTargetContains;
            var haystack = classifications[i].TargetAzureService ?? string.Empty;
            if (haystack.Contains(needle, StringComparison.OrdinalIgnoreCase))
                hits++;
            else
                misses.Add($"  {cases[i].Id}: expected target to contain \"{needle}\", got \"{haystack}\"");
        }

        var rate = (double)hits / cases.Count;
        Assert.True(
            rate >= TargetServiceContainsThreshold,
            $"Target-service contains rate {rate:P0} below threshold {TargetServiceContainsThreshold:P0}. Misses:\n{string.Join("\n", misses)}");
    }

    [Fact]
    public void Deterministic_GoldenConfidence_IsInValidRange()
    {
        var cases = LoadGolden();
        var stage = new ClassifierStage();
        var classifications = stage.Classify(cases.Select(c => c.ToWorkload()));

        Assert.All(classifications, c =>
        {
            Assert.InRange(c.Confidence, 0.0, 1.0);
            Assert.NotEqual(0.0, c.Confidence);
        });
    }

    [Fact]
    public void Deterministic_GoldenRationale_IsNonEmpty()
    {
        var cases = LoadGolden();
        var stage = new ClassifierStage();
        var classifications = stage.Classify(cases.Select(c => c.ToWorkload()));

        Assert.All(classifications, c => Assert.False(string.IsNullOrWhiteSpace(c.Rationale)));
    }

    private static IReadOnlyList<GoldenCase> LoadGolden()
    {
        var path = ResolveGoldenPath();
        var lines = File.ReadAllLines(path);
        return lines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => JsonSerializer.Deserialize<GoldenCase>(l, JsonOpts)
                         ?? throw new InvalidOperationException($"Bad golden line: {l}"))
            .ToList();
    }

    private static string ResolveGoldenPath()
    {
        var copied = Path.Combine(AppContext.BaseDirectory, "golden", "classifier-golden-v1.jsonl");
        if (File.Exists(copied)) return copied;

        // Fallback: walk up to repo root and read from samples/.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "samples", "golden", "classifier-golden-v1.jsonl");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException("Could not locate classifier-golden-v1.jsonl");
    }

    private sealed record GoldenCase
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

        public required ModernizationStrategy ExpectedStrategy { get; init; }
        public required string ExpectedTargetContains { get; init; }

        public Workload ToWorkload() => new()
        {
            Id = Id,
            ApplicationName = ApplicationName,
            Environment = Environment,
            OperatingSystem = OperatingSystem,
            VCpus = VCpus,
            MemoryGb = MemoryGb,
            StorageGb = StorageGb,
            DatabaseEngine = DatabaseEngine,
            MonthlyRequests = MonthlyRequests,
            PublicFacing = PublicFacing,
            Criticality = Criticality,
            CurrentMonthlyCostUsd = CurrentMonthlyCostUsd
        };
    }
}
