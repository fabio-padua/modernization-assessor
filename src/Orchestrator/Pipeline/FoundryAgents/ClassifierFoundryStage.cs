using System.Text.Json;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

/// <summary>
/// Agent-mode Classifier stage. Sends one workload at a time to the Foundry Classifier
/// agent and parses the returned JSON object back into a <see cref="Classification"/>.
/// </summary>
internal sealed class ClassifierFoundryStage : IClassifierStage
{
    private readonly AgentRunner _runner;
    private readonly PersistentAgent _agent;
    private readonly ILogger<ClassifierFoundryStage> _logger;

    private static readonly JsonSerializerOptions s_workloadJson = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ClassifierFoundryStage(
        AgentRunner runner,
        PersistentAgent agent,
        ILogger<ClassifierFoundryStage> logger)
    {
        _runner = runner;
        _agent = agent;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Classification>> ClassifyAsync(
        IReadOnlyList<Workload> workloads,
        CancellationToken ct)
    {
        var results = new List<Classification>(workloads.Count);
        for (var i = 0; i < workloads.Count; i++)
        {
            var w = workloads[i];
            _logger.LogInformation("Classifying [{Index}/{Total}] {App}", i + 1, workloads.Count, w.ApplicationName);

            var workloadJson = JsonSerializer.Serialize(w, s_workloadJson);
            var prompt = $"""
                Classify the following Workload per your instructions. Return ONE Classification JSON object.

                WORKLOAD:
                ```json
                {workloadJson}
                ```
                """;

            var response = await _runner.RunAsync(_agent, prompt, ct);
            _logger.LogDebug("Classifier agent raw output for {Id}: {Output}", w.Id, response);

            try
            {
                var classification = AgentJsonContracts.ParseClassification(response, w.Id);
                // Guarantee the WorkloadId matches; the agent may have echoed something different.
                if (!string.Equals(classification.WorkloadId, w.Id, StringComparison.Ordinal))
                {
                    classification = classification with { WorkloadId = w.Id };
                }
                results.Add(classification);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Classifier returned invalid JSON for {Id}. Raw: {Raw}", w.Id, response);
                throw new InvalidOperationException(
                    $"Classifier agent returned invalid JSON for workload {w.Id}. See logs.", ex);
            }
        }
        return results;
    }
}
