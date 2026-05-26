using System.Text.Json;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernizationAssessor.Orchestrator.Pipeline;
using ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;

namespace ModernizationAssessor.Orchestrator;

/// <summary>
/// CLI entrypoint.
///
///   dotnet run --project src/Orchestrator -- \
///     --inventory samples/inventory/contoso.csv \
///     --customer  Contoso \
///     --mode      deterministic | agents
///
/// In agents mode, the orchestrator connects to the Foundry project specified by
/// the <c>Foundry__ProjectEndpoint</c> and <c>Foundry__ModelDeploymentName</c>
/// environment variables (injected automatically when running in Container Apps).
///
/// Produces:
///   out/assessment-{timestamp}.json
///   out/assessment-{timestamp}.md
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            return await RunAsync(args);
        }
        catch (InvalidOperationException ex)
        {
            // Configuration / validation errors — show a clean message, not a stack trace.
            Console.Error.WriteLine();
            Console.Error.WriteLine("ERROR: " + ex.Message);
            Console.Error.WriteLine();
            return 1;
        }
    }

    private static async Task<int> RunAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        // Foundry options (env vars Foundry__ProjectEndpoint / Foundry__ModelDeploymentName,
        // or "Foundry" section in appsettings.json).
        var foundryOptions = new FoundryOptions();
        builder.Configuration.GetSection(FoundryOptions.SectionName).Bind(foundryOptions);
        builder.Services.AddSingleton(foundryOptions);

        var mode = ParseMode(builder.Configuration["mode"]);

        builder.Services.AddLogging(b => b.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
        }));

        // OpenTelemetry -> App Insights when a connection string is configured.
        // The Container Apps Job injects APPLICATIONINSIGHTS_CONNECTION_STRING automatically.
        var appInsightsConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            ?? builder.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(appInsightsConn))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(o => o.ConnectionString = appInsightsConn);
        }

        // Register the right stage implementations for the chosen mode.
        RegisterStages(builder.Services, mode, foundryOptions);

        builder.Services.AddSingleton<AssessmentPipeline>(sp => new AssessmentPipeline(
            sp.GetRequiredService<ILogger<AssessmentPipeline>>(),
            sp.GetRequiredService<IDiscoveryStage>(),
            sp.GetRequiredService<IClassifierStage>(),
            mode));

        using var host = builder.Build();
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

        logger.LogInformation("Modernization Assessor");
        logger.LogInformation("Mode        : {Mode}", mode);
        logger.LogInformation("Customer    : {Customer}", customerName);
        logger.LogInformation("Inventory   : {Inventory}", Path.GetFullPath(inventoryPath));

        try
        {
            var report = await pipeline.RunAsync(customerName, inventoryPath);

            var outDir = builder.Configuration["ASSESSOR_OUTPUT_DIR"] ?? "out";
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

            var blobUrl = builder.Configuration["ASSESSOR_OUTPUT_BLOB_URL"];
            if (!string.IsNullOrWhiteSpace(blobUrl) && Uri.TryCreate(blobUrl, UriKind.Absolute, out var containerUri))
            {
                logger.LogInformation("Publishing report artifacts to blob container {Url}", containerUri);
                var publisher = new BlobReportPublisher(
                    containerUri,
                    new DefaultAzureCredential(),
                    host.Services.GetRequiredService<ILogger<BlobReportPublisher>>());
                await publisher.PublishAsync(customerName, new[] { jsonPath, mdPath }, default);
            }

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Assessment run failed");
            return 2;
        }
    }

    private static RunMode ParseMode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return RunMode.Deterministic;
        return raw.Trim().ToLowerInvariant() switch
        {
            "agents" or "agent" or "foundry" => RunMode.Agents,
            "deterministic" or "rules" or "offline" => RunMode.Deterministic,
            _ => throw new InvalidOperationException(
                $"Unknown --mode value '{raw}'. Use 'deterministic' or 'agents'.")
        };
    }

    private static void RegisterStages(IServiceCollection services, RunMode mode, FoundryOptions options)
    {
        if (mode == RunMode.Deterministic)
        {
            services.AddSingleton<IDiscoveryStage, DiscoveryStage>();
            services.AddSingleton<IClassifierStage, ClassifierStage>();
            return;
        }

        options.ValidateForAgentsMode();

        // One PersistentAgentsClient per process. DefaultAzureCredential picks up
        // AZURE_CLIENT_ID (UAMI in Container Apps), az login locally, or workload-identity tokens.
        services.AddSingleton(_ =>
            FoundryAgentBootstrap.CreateClient(options, new DefaultAzureCredential()));

        // Cache the bootstrap result so both stages share the same PersistentAgent instances.
        services.AddSingleton<BootstrappedAgents>(sp =>
        {
            var client = sp.GetRequiredService<Azure.AI.Agents.Persistent.PersistentAgentsClient>();
            var bootstrap = new FoundryAgentBootstrap(
                client,
                options,
                sp.GetRequiredService<ILogger<FoundryAgentBootstrap>>());
            return bootstrap.EnsureAsync(CancellationToken.None).GetAwaiter().GetResult();
        });

        services.AddSingleton<IDiscoveryStage>(sp =>
        {
            var client = sp.GetRequiredService<Azure.AI.Agents.Persistent.PersistentAgentsClient>();
            var agents = sp.GetRequiredService<BootstrappedAgents>();
            var runner = new AgentRunner(client, options.RunTimeout, sp.GetRequiredService<ILogger<AgentRunner>>());
            return new DiscoveryFoundryStage(runner, agents.Discovery,
                sp.GetRequiredService<ILogger<DiscoveryFoundryStage>>());
        });

        services.AddSingleton<IClassifierStage>(sp =>
        {
            var client = sp.GetRequiredService<Azure.AI.Agents.Persistent.PersistentAgentsClient>();
            var agents = sp.GetRequiredService<BootstrappedAgents>();
            var runner = new AgentRunner(client, options.RunTimeout, sp.GetRequiredService<ILogger<AgentRunner>>());
            return new ClassifierFoundryStage(runner, agents.Classifier,
                sp.GetRequiredService<ILogger<ClassifierFoundryStage>>());
        });
    }
}
