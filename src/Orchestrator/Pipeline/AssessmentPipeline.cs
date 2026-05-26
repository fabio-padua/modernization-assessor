using Microsoft.Extensions.Logging;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

internal sealed class AssessmentPipeline
{
    private readonly ILogger<AssessmentPipeline> _logger;
    private readonly IDiscoveryStage _discovery;
    private readonly IClassifierStage _classifier;
    private readonly RunMode _mode;

    public AssessmentPipeline(
        ILogger<AssessmentPipeline> logger,
        IDiscoveryStage discovery,
        IClassifierStage classifier,
        RunMode mode)
    {
        _logger = logger;
        _discovery = discovery;
        _classifier = classifier;
        _mode = mode;
    }

    public async Task<AssessmentReport> RunAsync(
        string customerName,
        string inventoryPath,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Pipeline mode: {Mode}", _mode);

        _logger.LogInformation("[1/2] Discovery — normalizing inventory");
        var workloads = await _discovery.NormalizeAsync(inventoryPath, ct);
        _logger.LogInformation("      {Count} workloads parsed", workloads.Count);

        _logger.LogInformation("[2/2] Workload Classifier — strategy per workload");
        var classifications = await _classifier.ClassifyAsync(workloads, ct);
        _logger.LogInformation("      {Count} classifications produced", classifications.Count);

        var generatedBy = _mode == RunMode.Agents
            ? "modernization-assessor v0.2 (Foundry agents)"
            : "modernization-assessor v0.1 (deterministic stages)";

        return new AssessmentReport
        {
            CustomerName = customerName,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = generatedBy,
            Workloads = workloads,
            Classifications = classifications
        };
    }
}
