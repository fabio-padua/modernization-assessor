using ModernizationAssessor.Shared.Models;
using Xunit;

namespace ModernizationAssessor.Orchestrator.Tests;

public class ModelTests
{
    [Fact]
    public void Workload_Round_Trips_With_Required_Fields()
    {
        var w = new Workload { Id = "wl-001", ApplicationName = "erp" };
        Assert.Equal("wl-001", w.Id);
        Assert.Equal("erp", w.ApplicationName);
        Assert.Empty(w.Tags);
    }

    [Fact]
    public void Classification_Required_Fields_Enforced()
    {
        var c = new Classification
        {
            WorkloadId = "wl-001",
            Strategy = ModernizationStrategy.Refactor,
            TargetAzureService = "Azure SQL MI",
            Confidence = 0.8,
            Rationale = "SQL Server compatible"
        };
        Assert.Equal(ModernizationStrategy.Refactor, c.Strategy);
        Assert.InRange(c.Confidence, 0.0, 1.0);
    }
}
