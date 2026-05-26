using Azure.AI.Agents.Persistent;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

/// <summary>
/// Idempotently creates the v0.2 Foundry agents in the configured project.
/// If an agent with the configured name already exists, its id is reused.
/// Updating an existing agent's instructions when the prompt changes is a v0.2.x concern.
/// </summary>
internal sealed class FoundryAgentBootstrap
{
    private readonly PersistentAgentsClient _client;
    private readonly FoundryOptions _options;
    private readonly ILogger<FoundryAgentBootstrap> _logger;

    public FoundryAgentBootstrap(
        PersistentAgentsClient client,
        FoundryOptions options,
        ILogger<FoundryAgentBootstrap> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public static PersistentAgentsClient CreateClient(FoundryOptions options, TokenCredential credential)
    {
        options.ValidateForAgentsMode();
        return new PersistentAgentsClient(options.ProjectEndpoint!, credential);
    }

    public async Task<BootstrappedAgents> EnsureAsync(CancellationToken ct)
    {
        var discovery = await EnsureAgentAsync(
            _options.DiscoveryAgentName,
            AgentInstructions.DiscoveryAgentDescription,
            AgentInstructions.DiscoveryAgent,
            ct);

        var classifier = await EnsureAgentAsync(
            _options.ClassifierAgentName,
            AgentInstructions.ClassifierAgentDescription,
            AgentInstructions.ClassifierAgent,
            ct);

        return new BootstrappedAgents(discovery, classifier);
    }

    private async Task<PersistentAgent> EnsureAgentAsync(
        string name,
        string description,
        string instructions,
        CancellationToken ct)
    {
        await foreach (var existing in _client.Administration.GetAgentsAsync(
                           limit: null, order: null, after: null, before: null, cancellationToken: ct))
        {
            if (string.Equals(existing.Name, name, StringComparison.Ordinal))
            {
                _logger.LogInformation("Reusing Foundry agent '{Name}' (id={Id})", name, existing.Id);
                return existing;
            }
        }

        _logger.LogInformation("Creating Foundry agent '{Name}' on model '{Model}'", name, _options.ModelDeploymentName);
        var created = await _client.Administration.CreateAgentAsync(
            model: _options.ModelDeploymentName!,
            name: name,
            description: description,
            instructions: instructions,
            tools: null,
            toolResources: null,
            temperature: null,
            topP: null,
            responseFormat: null,
            metadata: null,
            cancellationToken: ct);

        _logger.LogInformation("Created Foundry agent '{Name}' (id={Id})", name, created.Value.Id);
        return created.Value;
    }
}

internal sealed record BootstrappedAgents(PersistentAgent Discovery, PersistentAgent Classifier);
