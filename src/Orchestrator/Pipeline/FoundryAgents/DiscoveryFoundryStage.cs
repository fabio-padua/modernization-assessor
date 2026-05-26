using System.Text.Json;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

/// <summary>
/// Agent-mode Discovery stage. Hands the raw inventory text to the Foundry Discovery
/// agent and parses the returned JSON array back into <see cref="Workload"/> records.
/// </summary>
internal sealed class DiscoveryFoundryStage : IDiscoveryStage
{
    private readonly AgentRunner _runner;
    private readonly PersistentAgent _agent;
    private readonly ILogger<DiscoveryFoundryStage> _logger;

    public DiscoveryFoundryStage(
        AgentRunner runner,
        PersistentAgent agent,
        ILogger<DiscoveryFoundryStage> logger)
    {
        _runner = runner;
        _agent = agent;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Workload>> NormalizeAsync(string inventoryPath, CancellationToken ct)
    {
        var raw = await File.ReadAllTextAsync(inventoryPath, ct);
        var prompt = $"""
            Normalize the following inventory into a JSON array of Workload objects per your instructions.

            INVENTORY:
            ```
            {raw}
            ```
            """;

        var response = await _runner.RunAsync(_agent, prompt, ct);
        _logger.LogDebug("Discovery agent raw output: {Output}", response);

        try
        {
            var workloads = AgentJsonContracts.ParseWorkloads(response);
            _logger.LogInformation("Discovery agent parsed {Count} workloads", workloads.Count);
            return workloads;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Discovery agent returned invalid JSON. Raw response: {Raw}", response);
            throw new InvalidOperationException(
                "Discovery agent returned invalid JSON. See logs for the raw response.", ex);
        }
    }
}
