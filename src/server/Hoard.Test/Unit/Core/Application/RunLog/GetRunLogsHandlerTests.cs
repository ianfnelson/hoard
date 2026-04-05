using Hoard.Core.Application.RunLog;
using Hoard.Test.Shared;
using RunLogEntity = Hoard.Core.Domain.Entities.RunLog;
using RunType = Hoard.Core.Domain.Entities.RunType;

namespace Hoard.Test.Unit.Core.Application.RunLog;

public class GetRunLogsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedResults()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var runType = new RunType { Id = 1, Name = "Close Of Day" };
        context.RunTypes.Add(runType);

        for (var i = 1; i <= 5; i++)
        {
            context.RunLogs.Add(new RunLogEntity
            {
                Id = i,
                RunTypeId = 1,
                RunId = Guid.NewGuid(),
                StartedAtUtc = new DateTime(2024, 1, i, 10, 0, 0, DateTimeKind.Utc),
                CompletedAtUtc = new DateTime(2024, 1, i, 10, 5, 0, DateTimeKind.Utc)
            });
        }
        await context.SaveChangesAsync();

        var handler = new GetRunLogsHandler(context);
        var query = new GetRunLogsQuery { PageNumber = 1, PageSize = 3 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersByRunTypeId()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        context.RunTypes.AddRange(
            new RunType { Id = 1, Name = "Close Of Day" },
            new RunType { Id = 2, Name = "Refresh Prices" }
        );
        context.RunLogs.AddRange(
            new RunLogEntity { Id = 1, RunTypeId = 1, RunId = Guid.NewGuid(), StartedAtUtc = DateTime.UtcNow },
            new RunLogEntity { Id = 2, RunTypeId = 1, RunId = Guid.NewGuid(), StartedAtUtc = DateTime.UtcNow },
            new RunLogEntity { Id = 3, RunTypeId = 2, RunId = Guid.NewGuid(), StartedAtUtc = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetRunLogsHandler(context);
        var query = new GetRunLogsQuery { RunTypeId = 1 };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(1, item.RunTypeId));
    }

    [Fact]
    public async Task HandleAsync_DefaultsSortByStartedAtDescending()
    {
        // Arrange
        await using var context = TestContextFactory.Create();
        var runType = new RunType { Id = 1, Name = "Close Of Day" };
        context.RunTypes.Add(runType);
        context.RunLogs.AddRange(
            new RunLogEntity { Id = 1, RunTypeId = 1, RunId = Guid.NewGuid(), StartedAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new RunLogEntity { Id = 2, RunTypeId = 1, RunId = Guid.NewGuid(), StartedAtUtc = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc) },
            new RunLogEntity { Id = 3, RunTypeId = 1, RunId = Guid.NewGuid(), StartedAtUtc = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc) }
        );
        await context.SaveChangesAsync();

        var handler = new GetRunLogsHandler(context);
        var query = new GetRunLogsQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.True(result.Items[0].StartedAtUtc > result.Items[1].StartedAtUtc);
        Assert.True(result.Items[1].StartedAtUtc > result.Items[2].StartedAtUtc);
    }

    [Fact]
    public async Task HandleAsync_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        await using var context = TestContextFactory.Create();
        var handler = new GetRunLogsHandler(context);

        var result = await handler.HandleAsync(new GetRunLogsQuery());

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

}
