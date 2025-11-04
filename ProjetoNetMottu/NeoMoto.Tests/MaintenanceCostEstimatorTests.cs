using FluentAssertions;
using NeoMoto.Api.Services;

namespace NeoMoto.Tests;

public class MaintenanceCostEstimatorTests
{
    [Fact]
    public void Predict_should_not_throw_exception()
    {
        using var svc = new MaintenanceCostEstimator();
        var features = new MaintenanceFeatures { AgeYears = 3, DaysSinceLastService = 60, ServiceType = 1 };
        
        Action act = () => svc.Predict(features);
        act.Should().NotThrow();
    }
}
