using Microsoft.Extensions.Logging;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

internal sealed class AssessmentPipeline
{
    private readonly ILogger<AssessmentPipeline> _logger;
    private readonly DiscoveryStage _discovery = new();
    private readonly ClassifierStage _classifier = new();

    public AssessmentPipeline(ILogger<AssessmentPipeline> logger) { _logger = logger; }

    public Task<AssessmentReport> RunAsync(string customerName, string inventoryPath)
    {
        _logger.LogInformation("[1/2] Discovery — normalizing inventory");
        var workloads = _discovery.Normalize(inventoryPath);
        _logger.LogInformation("      {Count} workloads parsed", workloads.Count);

        _logger.LogInformation("[2/2] Workload Classifier — strategy per workload");
        var classifications = _classifier.Classify(workloads);
        _logger.LogInformation("      {Count} classifications produced", classifications.Count);

        var report = new AssessmentReport
        {
            CustomerName = customerName,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = "modernization-assessor v0.1 (stand-in stages)",
            Workloads = workloads,
            Classifications = classifications
        };
        return Task.FromResult(report);
    }
}
