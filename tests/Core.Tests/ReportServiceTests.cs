using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    [Theory]
    [InlineData(20000, 2)]
    [InlineData(30000, 0)]
    public void HighValueOrders_CountsOnlyAboveThreshold(decimal threshold, int expectedCount)
    {
        // Act
        var result = ReportService.HighValueOrders(TestData.Orders(), threshold);

        // Assert
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, r => Assert.True(r.Total >= threshold));
    }

    [Fact]
    public void TopOrdersByTotal_IsDeterministic()
    {
        // Act: ORD-T02 та ORD-T04 мають однакову суму (25 000 грн).
        // ThenBy(Id, StringComparer.Ordinal) гарантує стабільний порядок за алфавітом: ORD-T02 перед ORD-T04.
        var top2 = ReportService.TopOrdersByTotal(TestData.Orders(), 2);

        // Assert
        var expectedIds = new[] { "ORD-T02", "ORD-T04" };
        Assert.Equal(expectedIds, top2.Select(r => r.OrderId));
    }

    [Fact]
    public void Totals_WhenEmpty_ReturnsZerosWithoutException()
    {
        // Act
        var (count, total, average, max) = ReportService.Totals(Enumerable.Empty<Order>());

        // Assert: безпечна обробка порожнього джерела без InvalidOperationException
        Assert.Equal(0, count);
        Assert.Equal(0m, total);
        Assert.Equal(0m, average);
        Assert.Equal(0m, max);
    }
}
