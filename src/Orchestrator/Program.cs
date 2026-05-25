using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernizationAssessor.Orchestrator.Pipeline;

namespace ModernizationAssessor.Orchestrator;

/// <summary>
/// CLI entrypoint for the v0.1 thin slice.
///
///   dotnet run --project src/Orchestrator -- --inventory samples/inventory/contoso.csv --customer Contoso
///
/// Produces:
///   out/assessment-{timestamp}.json
///   out/assessment-{timestamp}.md
///
/// v0.2 will add an HTTP host (Foundry hosting adapter) so the same pipeline
/// runs behind APIM in ACA without code changes.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.Services.AddSingleton<AssessmentPipeline>();
        builder.Services.AddLogging(b => b.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
        }));

        var host = builder.Build();
        var pipeline = host.Services.GetRequiredService<AssessmentPipeline>();
        var logger = host.Services.GetRequiredService<ILogger<object>>();

        var inventoryPath = builder.Configuration["inventory"]
            ?? throw new InvalidOperationException("Missing --inventory <path-to-csv>");
        var customerName = builder.Configuration["customer"] ?? "Sample Customer";

        if (!File.Exists(inventoryPath))
        {
            logger.LogError("Inventory file not found: {Path}", inventoryPath);
            return 1;
        }

        logger.LogInformation("Modernization Assessor — v0.1");
        logger.LogInformation("Customer    : {Customer}", customerName);
        logger.LogInformation("Inventory   : {Inventory}", Path.GetFullPath(inventoryPath));

        var report = await pipeline.RunAsync(customerName, inventoryPath);

        var outDir = "out";
        Directory.CreateDirectory(outDir);
        var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var jsonPath = Path.Combine(outDir, $"assessment-{stamp}.json");
        var mdPath = Path.Combine(outDir, $"assessment-{stamp}.md");

        await File.WriteAllTextAsync(
            jsonPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        await File.WriteAllTextAsync(mdPath, MarkdownRenderer.Render(report));

        logger.LogInformation("Wrote {Json}", jsonPath);
        logger.LogInformation("Wrote {Md}", mdPath);
        return 0;
    }
}
