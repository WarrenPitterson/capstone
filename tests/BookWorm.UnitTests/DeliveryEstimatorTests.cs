using BookWorm.Api.Options;
using BookWorm.Api.Services.Implementations;
using Microsoft.Extensions.Options;
using Xunit;

namespace BookWorm.UnitTests;

public class DeliveryEstimatorTests
{
    private readonly DeliveryEstimator _sut;

    public DeliveryEstimatorTests()
    {
        var pricingOptions = Options.Create(new PricingOptions
        {
            EstimatedDaysToDeliver = 3
        });
        _sut = new DeliveryEstimator(pricingOptions);
    }

    [Fact]
    public void EstimateDeliveryDate_FromThursday_SkipsSundayAndReturnsMonday()
    {
        // Arrange: Thursday, July 17, 2026 (+1 Fri, +2 Sat, Sunday skipped, +3 Mon Jul 21)
        var thursday = new DateTime(2026, 7, 16);

        // Act
        var result = _sut.EstimateDeliveryDate(thursday);

        // Assert
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.Equal(new DateTime(2026, 7, 20), result);
    }

    [Fact]
    public void EstimateDeliveryDate_FromMonday_ReturnsThursday()
    {
        // Arrange: Monday, July 13, 2026 (+1 Tue, +2 Wed, +3 Thu)
        var monday = new DateTime(2026, 7, 13);

        // Act
        var result = _sut.EstimateDeliveryDate(monday);

        // Assert
        Assert.Equal(DayOfWeek.Thursday, result.DayOfWeek);
        Assert.Equal(new DateTime(2026, 7, 16), result);
    }
}
