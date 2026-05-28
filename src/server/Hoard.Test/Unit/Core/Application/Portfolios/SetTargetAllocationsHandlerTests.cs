using Hoard.Core.Application.Portfolios;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class SetTargetAllocationsHandlerTests
{
    private static SetTargetAllocationsRequest Request(params (int subclassId, decimal target)[] items) =>
        new()
        {
            Allocations = items
                .Select(i => new TargetAllocationItemDto { AssetSubclassId = i.subclassId, Target = i.target })
                .ToList()
        };

    [Fact]
    public async Task PortfolioNotFound_Throws()
    {
        await using var context = TestContextFactory.Create();
        var handler = new SetTargetAllocationsHandler(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new SetTargetAllocationsCommand(999, Request((10, 100m)))));
    }

    [Fact]
    public async Task ReplacesExistingAllocations()
    {
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        context.TargetAllocations.AddRange(
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 10, Target = 30m },
            new TargetAllocation { PortfolioId = 1, AssetSubclassId = 20, Target = 70m });
        await context.SaveChangesAsync();

        var handler = new SetTargetAllocationsHandler(context);
        await handler.HandleAsync(new SetTargetAllocationsCommand(1, Request((10, 100m))));

        var stored = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == 1)
            .ToListAsync();

        Assert.Single(stored);
        Assert.Equal(10, stored[0].AssetSubclassId);
        Assert.Equal(100m, stored[0].Target);
    }

    [Fact]
    public async Task PersistsOnlyNonZeroEntries()
    {
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var handler = new SetTargetAllocationsHandler(context);
        await handler.HandleAsync(new SetTargetAllocationsCommand(1, Request((10, 100m), (20, 0m))));

        var stored = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == 1)
            .ToListAsync();

        Assert.DoesNotContain(stored, ta => ta.AssetSubclassId == 20);
    }

    [Fact]
    public async Task ComputesCashRemainder()
    {
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var handler = new SetTargetAllocationsHandler(context);
        await handler.HandleAsync(new SetTargetAllocationsCommand(1, Request((10, 50m), (20, 30m))));

        var stored = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == 1)
            .ToListAsync();

        var cash = Assert.Single(stored, ta => ta.AssetSubclassId == AssetSubclass.Cash);
        Assert.Equal(20m, cash.Target);
    }

    [Fact]
    public async Task SumExactly100_StoresNoCashRow()
    {
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var handler = new SetTargetAllocationsHandler(context);
        await handler.HandleAsync(new SetTargetAllocationsCommand(1, Request((10, 60m), (20, 40m))));

        var stored = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == 1)
            .ToListAsync();

        Assert.DoesNotContain(stored, ta => ta.AssetSubclassId == AssetSubclass.Cash);
        Assert.Equal(2, stored.Count);
    }

    [Fact]
    public async Task EmptyAllocations_AllocatesEverythingToCash()
    {
        await using var context = TestContextFactory.Create();
        context.Portfolios.Add(TestData.Portfolio());
        await context.SaveChangesAsync();

        var handler = new SetTargetAllocationsHandler(context);
        await handler.HandleAsync(new SetTargetAllocationsCommand(1, Request()));

        var stored = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == 1)
            .ToListAsync();

        var cash = Assert.Single(stored);
        Assert.Equal(AssetSubclass.Cash, cash.AssetSubclassId);
        Assert.Equal(100m, cash.Target);
    }
}
