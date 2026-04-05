using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class GetAlertsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.AlertTypes.Add(new AlertType { Id = 1, Name = "Price Movement" });
        for (var i = 1; i <= 5; i++)
        {
            context.Alerts.Add(new Alert
            {
                Id = i, AlertTypeId = 1, TriggeredDate = new DateOnly(2024, 1, i),
                CreatedUtc = new DateTime(2024, 1, i, 10, 0, 0, DateTimeKind.Utc),
                SuppressionKey = $"key-{i}", Title = $"Alert {i}", Message = $"Message {i}"
            });
        }
        await context.SaveChangesAsync();

        var handler = new GetAlertsHandler(context);
        var query = new GetAlertsQuery { PageNumber = 1, PageSize = 3 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersByAlertTypeId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.AlertTypes.AddRange(
            new AlertType { Id = 1, Name = "Price Movement" },
            new AlertType { Id = 2, Name = "News" }
        );
        context.Alerts.AddRange(
            new Alert { Id = 1, AlertTypeId = 1, TriggeredDate = new DateOnly(2024, 1, 1), CreatedUtc = DateTime.UtcNow, SuppressionKey = "k1", Title = "A1", Message = "M1" },
            new Alert { Id = 2, AlertTypeId = 2, TriggeredDate = new DateOnly(2024, 1, 1), CreatedUtc = DateTime.UtcNow, SuppressionKey = "k2", Title = "A2", Message = "M2" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAlertsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAlertsQuery { AlertTypeId = 1 });

        // Assert
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_SearchByTitle()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.AlertTypes.Add(new AlertType { Id = 1, Name = "Price Movement" });
        context.Alerts.AddRange(
            new Alert { Id = 1, AlertTypeId = 1, TriggeredDate = new DateOnly(2024, 1, 1), CreatedUtc = DateTime.UtcNow, SuppressionKey = "k1", Title = "Fundsmith alert", Message = "M1" },
            new Alert { Id = 2, AlertTypeId = 1, TriggeredDate = new DateOnly(2024, 1, 1), CreatedUtc = DateTime.UtcNow, SuppressionKey = "k2", Title = "Vanguard alert", Message = "M2" }
        );
        await context.SaveChangesAsync();

        var handler = new GetAlertsHandler(context);

        // Act
        var result = await handler.HandleAsync(new GetAlertsQuery { Search = "Fundsmith" });

        // Assert
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetAlertsHandler(context);

        var result = await handler.HandleAsync(new GetAlertsQuery());

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}
