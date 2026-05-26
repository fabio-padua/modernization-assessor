using System.Text.Json;
using System.Text.Json.Serialization;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

/// <summary>
/// DTOs and helpers for parsing agent output back into domain records.
/// Kept separate from the domain types so the wire shape can evolve
/// independently of <see cref="Workload"/> / <see cref="Classification"/>.
/// </summary>
internal static class AgentJsonContracts
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    /// <summary>
    /// Strips ``` and ```json code fences if the model wrapped the JSON.
    /// Returns the substring from the first '{' or '[' to the matching last brace.
    /// </summary>
    public static string ExtractJsonPayload(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Agent returned an empty response.");

        var text = raw.Trim();

        // Strip ```json ... ``` or ``` ... ``` fences.
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0) text = text[(firstNewline + 1)..];
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0) text = text[..lastFence];
            text = text.Trim();
        }

        // Find outermost JSON value.
        var firstBrace = text.IndexOfAny(new[] { '{', '[' });
        if (firstBrace < 0)
            throw new InvalidOperationException("Agent response did not contain a JSON object or array.");

        var lastBrace = Math.Max(text.LastIndexOf('}'), text.LastIndexOf(']'));
        if (lastBrace <= firstBrace)
            throw new InvalidOperationException("Agent response was not balanced JSON.");

        return text[firstBrace..(lastBrace + 1)];
    }

    public static IReadOnlyList<Workload> ParseWorkloads(string raw)
    {
        var json = ExtractJsonPayload(raw);
        var dtos = JsonSerializer.Deserialize<List<WorkloadDto>>(json, JsonOptions)
                   ?? throw new InvalidOperationException("Discovery agent returned a null workload list.");

        var result = new List<Workload>(dtos.Count);
        var index = 0;
        foreach (var dto in dtos)
        {
            index++;
            if (string.IsNullOrWhiteSpace(dto.ApplicationName))
                throw new InvalidOperationException($"Workload at position {index} has no applicationName.");

            result.Add(new Workload
            {
                Id = string.IsNullOrWhiteSpace(dto.Id) ? $"wl-{index:D3}" : dto.Id!,
                ApplicationName = dto.ApplicationName!,
                Environment = dto.Environment,
                OperatingSystem = dto.OperatingSystem,
                VCpus = dto.VCpus,
                MemoryGb = dto.MemoryGb,
                StorageGb = dto.StorageGb,
                DatabaseEngine = dto.DatabaseEngine,
                MonthlyRequests = dto.MonthlyRequests,
                PublicFacing = dto.PublicFacing,
                Criticality = ParseCriticality(dto.Criticality),
                CurrentMonthlyCostUsd = dto.CurrentMonthlyCostUsd
            });
        }
        return result;
    }

    public static Classification ParseClassification(string raw, string fallbackWorkloadId)
    {
        var json = ExtractJsonPayload(raw);
        var dto = JsonSerializer.Deserialize<ClassificationDto>(json, JsonOptions)
                  ?? throw new InvalidOperationException("Classifier agent returned a null classification.");

        if (string.IsNullOrWhiteSpace(dto.Strategy))
            throw new InvalidOperationException("Classifier output missing 'strategy'.");
        if (string.IsNullOrWhiteSpace(dto.TargetAzureService))
            throw new InvalidOperationException("Classifier output missing 'targetAzureService'.");
        if (string.IsNullOrWhiteSpace(dto.Rationale))
            throw new InvalidOperationException("Classifier output missing 'rationale'.");

        var strategy = ParseStrategy(dto.Strategy!);
        var confidence = Math.Clamp(dto.Confidence ?? 0.5, 0.0, 1.0);

        return new Classification
        {
            WorkloadId = string.IsNullOrWhiteSpace(dto.WorkloadId) ? fallbackWorkloadId : dto.WorkloadId!,
            Strategy = strategy,
            TargetAzureService = dto.TargetAzureService!,
            Confidence = confidence,
            Rationale = dto.Rationale!,
            Risks = dto.Risks ?? Array.Empty<string>(),
            Prerequisites = dto.Prerequisites ?? Array.Empty<string>()
        };
    }

    private static BusinessCriticality? ParseCriticality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim().ToLowerInvariant() switch
        {
            "low" => BusinessCriticality.Low,
            "medium" or "med" => BusinessCriticality.Medium,
            "high" => BusinessCriticality.High,
            "critical" or "crit" => BusinessCriticality.Critical,
            _ => null
        };
    }

    private static ModernizationStrategy ParseStrategy(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "rehost" => ModernizationStrategy.Rehost,
            "replatform" => ModernizationStrategy.Replatform,
            "refactor" => ModernizationStrategy.Refactor,
            "rebuild" => ModernizationStrategy.Rebuild,
            "replace" => ModernizationStrategy.Replace,
            "retire" => ModernizationStrategy.Retire,
            "retain" => ModernizationStrategy.Retain,
            _ => throw new InvalidOperationException(
                $"Classifier returned an unrecognized strategy '{value}'. Must be one of the 7 R's.")
        };
    }

    private sealed class WorkloadDto
    {
        public string? Id { get; set; }
        public string? ApplicationName { get; set; }
        public string? Environment { get; set; }
        public string? OperatingSystem { get; set; }
        public int? VCpus { get; set; }
        public double? MemoryGb { get; set; }
        public double? StorageGb { get; set; }
        public string? DatabaseEngine { get; set; }
        public long? MonthlyRequests { get; set; }
        public bool? PublicFacing { get; set; }
        public string? Criticality { get; set; }
        public decimal? CurrentMonthlyCostUsd { get; set; }
    }

    private sealed class ClassificationDto
    {
        public string? WorkloadId { get; set; }
        public string? Strategy { get; set; }
        public string? TargetAzureService { get; set; }
        public double? Confidence { get; set; }
        public string? Rationale { get; set; }
        public string[]? Risks { get; set; }
        public string[]? Prerequisites { get; set; }
    }
}
