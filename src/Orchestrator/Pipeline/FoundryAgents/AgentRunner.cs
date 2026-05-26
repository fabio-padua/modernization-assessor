using System.Linq;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;

namespace ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

/// <summary>
/// Helper that drives a single agent turn (create thread -> post user message ->
/// create run -> poll until terminal -> read newest assistant message).
/// </summary>
internal sealed class AgentRunner
{
    private readonly PersistentAgentsClient _client;
    private readonly TimeSpan _runTimeout;
    private readonly ILogger _logger;

    public AgentRunner(PersistentAgentsClient client, TimeSpan runTimeout, ILogger logger)
    {
        _client = client;
        _runTimeout = runTimeout;
        _logger = logger;
    }

    public async Task<string> RunAsync(PersistentAgent agent, string userInput, CancellationToken ct)
    {
        var thread = await _client.Threads.CreateThreadAsync(
            messages: null, toolResources: null, metadata: null, cancellationToken: ct);

        await _client.Messages.CreateMessageAsync(
            thread.Value.Id,
            MessageRole.User,
            userInput,
            attachments: null,
            metadata: null,
            cancellationToken: ct);

        var run = await _client.Runs.CreateRunAsync(thread.Value, agent, cancellationToken: ct);

        var deadline = DateTimeOffset.UtcNow + _runTimeout;
        while (run.Value.Status == RunStatus.Queued
               || run.Value.Status == RunStatus.InProgress
               || run.Value.Status == RunStatus.RequiresAction)
        {
            if (DateTimeOffset.UtcNow > deadline)
            {
                _logger.LogWarning("Agent run {RunId} exceeded timeout {Timeout}, cancelling",
                    run.Value.Id, _runTimeout);
                try { await _client.Runs.CancelRunAsync(thread.Value.Id, run.Value.Id, ct); }
                catch { /* best-effort */ }
                throw new TimeoutException($"Foundry agent '{agent.Name}' run did not complete within {_runTimeout}.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), ct);
            run = await _client.Runs.GetRunAsync(thread.Value.Id, run.Value.Id, ct);
        }

        if (run.Value.Status != RunStatus.Completed)
        {
            var err = run.Value.LastError?.Message ?? "(no error message returned)";
            throw new InvalidOperationException(
                $"Foundry agent '{agent.Name}' run ended in status {run.Value.Status}: {err}");
        }

        await foreach (var msg in _client.Messages.GetMessagesAsync(
                           thread.Value.Id,
                           runId: run.Value.Id,
                           limit: null,
                           order: ListSortOrder.Descending,
                           after: null,
                           before: null,
                           cancellationToken: ct))
        {
            if (msg.Role != MessageRole.Agent) continue;
            var text = string.Concat(msg.ContentItems.OfType<MessageTextContent>().Select(c => c.Text));
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        throw new InvalidOperationException(
            $"Foundry agent '{agent.Name}' returned no assistant text content.");
    }
}
