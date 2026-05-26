using ModernizationAssessor.Orchestrator.Pipeline.FoundryAgents;
using ModernizationAssessor.Shared.Models;
using Xunit;

namespace ModernizationAssessor.Orchestrator.Tests;

public class AgentJsonContractsTests
{
    [Fact]
    public void ExtractJsonPayload_Strips_Json_Code_Fence()
    {
        var raw = "```json\n{\"a\":1}\n```";
        var json = AgentJsonContracts.ExtractJsonPayload(raw);
        Assert.Equal("{\"a\":1}", json);
    }

    [Fact]
    public void ExtractJsonPayload_Strips_Plain_Code_Fence()
    {
        var raw = "```\n[1,2,3]\n```";
        var json = AgentJsonContracts.ExtractJsonPayload(raw);
        Assert.Equal("[1,2,3]", json);
    }

    [Fact]
    public void ExtractJsonPayload_Tolerates_Leading_And_Trailing_Prose()
    {
        var raw = "Sure, here you go:\n{\"x\": \"y\"}\nLet me know if you need anything else.";
        var json = AgentJsonContracts.ExtractJsonPayload(raw);
        Assert.Equal("{\"x\": \"y\"}", json);
    }

    [Fact]
    public void ExtractJsonPayload_Throws_On_Empty()
    {
        Assert.Throws<InvalidOperationException>(() => AgentJsonContracts.ExtractJsonPayload(""));
    }

    [Fact]
    public void ExtractJsonPayload_Throws_When_No_Braces()
    {
        Assert.Throws<InvalidOperationException>(() => AgentJsonContracts.ExtractJsonPayload("no json here"));
    }

    [Fact]
    public void ParseWorkloads_Maps_All_Fields()
    {
        var json = """
            [
              {
                "id": "wl-001",
                "applicationName": "contoso-erp-prod",
                "environment": "prod",
                "operatingSystem": "WindowsServer2012",
                "vCpus": 16,
                "memoryGb": 64,
                "storageGb": 2048,
                "databaseEngine": "SQLServer2014",
                "monthlyRequests": 2500000,
                "publicFacing": false,
                "criticality": "High",
                "currentMonthlyCostUsd": 4200
              }
            ]
            """;

        var workloads = AgentJsonContracts.ParseWorkloads(json);

        Assert.Single(workloads);
        var w = workloads[0];
        Assert.Equal("wl-001", w.Id);
        Assert.Equal("contoso-erp-prod", w.ApplicationName);
        Assert.Equal("prod", w.Environment);
        Assert.Equal("WindowsServer2012", w.OperatingSystem);
        Assert.Equal(16, w.VCpus);
        Assert.Equal(64, w.MemoryGb);
        Assert.Equal(2048, w.StorageGb);
        Assert.Equal("SQLServer2014", w.DatabaseEngine);
        Assert.Equal(2_500_000L, w.MonthlyRequests);
        Assert.False(w.PublicFacing);
        Assert.Equal(BusinessCriticality.High, w.Criticality);
        Assert.Equal(4200m, w.CurrentMonthlyCostUsd);
    }

    [Fact]
    public void ParseWorkloads_Assigns_Sequential_Ids_When_Missing()
    {
        var json = """
            [
              {"applicationName": "app-a"},
              {"applicationName": "app-b"}
            ]
            """;

        var workloads = AgentJsonContracts.ParseWorkloads(json);

        Assert.Equal(2, workloads.Count);
        Assert.Equal("wl-001", workloads[0].Id);
        Assert.Equal("wl-002", workloads[1].Id);
    }

    [Fact]
    public void ParseWorkloads_Throws_When_ApplicationName_Missing()
    {
        var json = "[{\"id\": \"wl-001\"}]";
        Assert.Throws<InvalidOperationException>(() => AgentJsonContracts.ParseWorkloads(json));
    }

    [Theory]
    [InlineData("low", BusinessCriticality.Low)]
    [InlineData("Medium", BusinessCriticality.Medium)]
    [InlineData("HIGH", BusinessCriticality.High)]
    [InlineData("critical", BusinessCriticality.Critical)]
    public void ParseWorkloads_Maps_Criticality_Case_Insensitively(string raw, BusinessCriticality expected)
    {
        var json = $"[{{\"applicationName\": \"app\", \"criticality\": \"{raw}\"}}]";
        var workloads = AgentJsonContracts.ParseWorkloads(json);
        Assert.Equal(expected, workloads[0].Criticality);
    }

    [Fact]
    public void ParseClassification_Maps_All_Fields()
    {
        var json = """
            {
              "workloadId": "wl-006",
              "strategy": "Replatform",
              "targetAzureService": "Azure Container Apps",
              "confidence": 0.85,
              "rationale": "Public-facing Ubuntu HTTP workload — ACA fits.",
              "risks": ["Containerization effort"],
              "prerequisites": ["Dockerfile", "Externalize config"]
            }
            """;

        var c = AgentJsonContracts.ParseClassification(json, fallbackWorkloadId: "wl-FALLBACK");

        Assert.Equal("wl-006", c.WorkloadId);
        Assert.Equal(ModernizationStrategy.Replatform, c.Strategy);
        Assert.Equal("Azure Container Apps", c.TargetAzureService);
        Assert.Equal(0.85, c.Confidence);
        Assert.Contains("Public-facing", c.Rationale);
        Assert.Single(c.Risks);
        Assert.Equal(2, c.Prerequisites.Count);
    }

    [Fact]
    public void ParseClassification_Uses_Fallback_Id_When_Missing()
    {
        var json = """
            {
              "strategy": "Rehost",
              "targetAzureService": "Azure VM",
              "confidence": 0.6,
              "rationale": "No PaaS signal."
            }
            """;

        var c = AgentJsonContracts.ParseClassification(json, fallbackWorkloadId: "wl-042");

        Assert.Equal("wl-042", c.WorkloadId);
        Assert.Equal(ModernizationStrategy.Rehost, c.Strategy);
    }

    [Fact]
    public void ParseClassification_Clamps_Confidence_To_Unit_Range()
    {
        var jsonHigh = """{"strategy":"Retire","targetAzureService":"(none)","confidence":1.7,"rationale":"x"}""";
        var jsonLow = """{"strategy":"Retire","targetAzureService":"(none)","confidence":-0.3,"rationale":"x"}""";

        Assert.Equal(1.0, AgentJsonContracts.ParseClassification(jsonHigh, "wl-1").Confidence);
        Assert.Equal(0.0, AgentJsonContracts.ParseClassification(jsonLow, "wl-1").Confidence);
    }

    [Theory]
    [InlineData("Rehost", ModernizationStrategy.Rehost)]
    [InlineData("REPLATFORM", ModernizationStrategy.Replatform)]
    [InlineData("refactor", ModernizationStrategy.Refactor)]
    [InlineData("Rebuild", ModernizationStrategy.Rebuild)]
    [InlineData("replace", ModernizationStrategy.Replace)]
    [InlineData("Retire", ModernizationStrategy.Retire)]
    [InlineData("retain", ModernizationStrategy.Retain)]
    public void ParseClassification_Maps_All_Seven_Strategies(string raw, ModernizationStrategy expected)
    {
        var json = $$"""
            {"strategy":"{{raw}}","targetAzureService":"x","confidence":0.5,"rationale":"y"}
            """;
        var c = AgentJsonContracts.ParseClassification(json, "wl-1");
        Assert.Equal(expected, c.Strategy);
    }

    [Fact]
    public void ParseClassification_Throws_On_Unknown_Strategy()
    {
        var json = """
            {"strategy":"Relocate","targetAzureService":"x","confidence":0.5,"rationale":"y"}
            """;
        Assert.Throws<InvalidOperationException>(
            () => AgentJsonContracts.ParseClassification(json, "wl-1"));
    }
}
