using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class GetAlertHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingAlert_ReturnsDetail()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.AlertTypes.Add(new AlertType { Id = 1, Name = "Price Movement" });
        context.Alerts.Add(new Alert
        {
            Id = 1, AlertTypeId = 1, TriggeredDate = new DateOnly(2024, 1, 15),
            CreatedUtc = DateTime.UtcNow, SuppressionKey = "test-key",
            Title = "Test Alert", Message = "Price moved"
        });
        await context.SaveChangesAsync();

        var handler = new GetAlertHandler(context, NullLogger<GetAlertHandler>.Instance);

        // Act
        var result = await handler.HandleAsync(new GetAlertQuery(1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Alert", result.Title);
        Assert.Equal("Price Movement", result.AlertTypeName);
    }

    [Fact]
    public async Task HandleAsync_NonExistentAlert_ReturnsNull()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetAlertHandler(context, NullLogger<GetAlertHandler>.Instance);

        var result = await handler.HandleAsync(new GetAlertQuery(999));

        Assert.Null(result);
    }
}
