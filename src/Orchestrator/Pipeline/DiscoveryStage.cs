using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ModernizationAssessor.Shared.Models;

namespace ModernizationAssessor.Orchestrator.Pipeline;

internal sealed class DiscoveryStage : IDiscoveryStage
{
    public Task<IReadOnlyList<Workload>> NormalizeAsync(string inventoryPath, CancellationToken ct)
        => Task.FromResult(Normalize(inventoryPath));

    public IReadOnlyList<Workload> Normalize(string csvPath)
    {
        using var reader = new StreamReader(csvPath);
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = h => h.Header.Trim().ToLowerInvariant()
        };
        using var csv = new CsvReader(reader, cfg);
        csv.Read();
        csv.ReadHeader();

        var workloads = new List<Workload>();
        while (csv.Read())
        {
            var name = csv.GetField("applicationname") ?? "";
            var crit = csv.GetField("businesscriticality")?.ToLowerInvariant();
            workloads.Add(new Workload
            {
                Id = $"wl-{workloads.Count + 1:D3}",
                ApplicationName = name,
                Environment = csv.GetField("environment"),
                OperatingSystem = csv.GetField("os"),
                VCpus = ParseInt(csv.GetField("vcpus")),
                MemoryGb = ParseDouble(csv.GetField("memorygb")),
                StorageGb = ParseDouble(csv.GetField("storagegb")),
                DatabaseEngine = NullIfBlank(csv.GetField("databaseengine")),
                MonthlyRequests = ParseLong(csv.GetField("monthlyrequests")),
                PublicFacing = ParseBool(csv.GetField("publicfacing")),
                Criticality = crit switch
                {
                    "low" => BusinessCriticality.Low,
                    "medium" => BusinessCriticality.Medium,
                    "high" => BusinessCriticality.High,
                    "critical" => BusinessCriticality.Critical,
                    _ => null
                },
                CurrentMonthlyCostUsd = ParseDecimal(csv.GetField("currentmonthlycostusd"))
            });
        }
        return workloads;
    }

    private static int? ParseInt(string? s) => int.TryParse(s, out var v) ? v : null;
    private static long? ParseLong(string? s) => long.TryParse(s, out var v) ? v : null;
    private static double? ParseDouble(string? s) =>
        double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    private static decimal? ParseDecimal(string? s) =>
        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    private static bool? ParseBool(string? s) => bool.TryParse(s, out var v) ? v : null;
    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
