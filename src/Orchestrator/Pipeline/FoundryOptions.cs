namespace ModernizationAssessor.Orchestrator.Pipeline;

/// <summary>
/// Strongly-typed configuration for the Foundry project that hosts the agents.
/// Populated from environment variables (preferred, set by Container Apps) or
/// <c>appsettings.json</c> under the <c>Foundry</c> section.
/// </summary>
public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    /// <summary>
    /// Full project endpoint, e.g.
    /// <c>https://contoso.services.ai.azure.com/api/projects/modassessor</c>.
    /// Surfaced by the deployment as <c>Foundry__ProjectEndpoint</c>.
    /// </summary>
    public string? ProjectEndpoint { get; set; }

    /// <summary>
    /// Name of the model deployment inside the Foundry project, e.g. <c>gpt-4.1</c>.
    /// Surfaced as <c>Foundry__ModelDeploymentName</c>.
    /// </summary>
    public string? ModelDeploymentName { get; set; }

    /// <summary>
    /// Stable name used to look up (or create) the Discovery agent.
    /// </summary>
    public string DiscoveryAgentName { get; set; } = "modassessor-discovery";

    /// <summary>
    /// Stable name used to look up (or create) the Workload Classifier agent.
    /// </summary>
    public string ClassifierAgentName { get; set; } = "modassessor-classifier";

    /// <summary>
    /// Hard timeout for a single agent run before the orchestrator gives up.
    /// </summary>
    public TimeSpan RunTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Throws when required values for Agents mode are missing.
    /// </summary>
    public void ValidateForAgentsMode()
    {
        if (string.IsNullOrWhiteSpace(ProjectEndpoint))
            throw new InvalidOperationException(
                "Foundry__ProjectEndpoint is not configured. Set it in appsettings.json or as an environment variable before running with --mode=agents.");
        if (string.IsNullOrWhiteSpace(ModelDeploymentName))
            throw new InvalidOperationException(
                "Foundry__ModelDeploymentName is not configured. Set it in appsettings.json or as an environment variable before running with --mode=agents.");
    }
}
