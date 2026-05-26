namespace ModernizationAssessor.Orchestrator.Pipeline;

/// <summary>
/// Controls how the Discovery and Classifier stages execute.
/// </summary>
public enum RunMode
{
    /// <summary>
    /// v0.1 behavior — deterministic C# rules. No network calls. Default for offline / CI / unit tests.
    /// </summary>
    Deterministic,

    /// <summary>
    /// v0.2 behavior — delegate normalization and classification to Foundry agents
    /// using the project endpoint configured via <c>Foundry__ProjectEndpoint</c>.
    /// </summary>
    Agents
}
