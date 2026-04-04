using System.Reflection;
using Hoard.Core.Application.Positions;
using Hoard.Core.Domain.Entities;

namespace Hoard.Test.Unit.Core.Application.Positions;

public class ProcessCalculatePositionsHandlerTests
{
    // BuildPositions is private static, so we test it via reflection.
    // This contains the real business logic for determining open/close dates.
    private static List<Position> InvokeBuildPositions(int portfolioId, object holdings)
    {
        var method = typeof(ProcessCalculatePositionsHandler)
            .GetMethod("BuildPositions", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (List<Position>)method.Invoke(null, [portfolioId, holdings])!;
    }

    private static object CreateAggregatedHolding(int instrumentId, DateOnly asOfDate, decimal units)
    {
        var type = typeof(ProcessCalculatePositionsHandler)
            .GetNestedType("AggregatedHolding", BindingFlags.NonPublic)!;
        return Activator.CreateInstance(type, instrumentId, asOfDate, units)!;
    }

    private static object CreateHoldingsList(params (int instrumentId, DateOnly date, decimal units)[] items)
    {
        var type = typeof(ProcessCalculatePositionsHandler)
            .GetNestedType("AggregatedHolding", BindingFlags.NonPublic)!;
        var listType = typeof(List<>).MakeGenericType(type);
        var list = Activator.CreateInstance(listType)!;
        var addMethod = listType.GetMethod("Add")!;

        foreach (var (instrumentId, date, units) in items)
        {
            addMethod.Invoke(list, [CreateAggregatedHolding(instrumentId, date, units)]);
        }

        return list;
    }

    [Fact]
    public void BuildPositions_SingleInstrumentStillOpen_CreatesOpenPosition()
    {
        // Consecutive dates to avoid gap-based closure
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 10), 10m),
            (1, new DateOnly(2024, 1, 11), 10m));

        var positions = InvokeBuildPositions(1, holdings);

        Assert.Single(positions);
        Assert.Equal(1, positions[0].InstrumentId);
        Assert.Equal(new DateOnly(2024, 1, 10), positions[0].OpenDate);
        Assert.Null(positions[0].CloseDate);
    }

    [Fact]
    public void BuildPositions_InstrumentSoldToZero_CreatesClosedPosition()
    {
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 10), 10m),
            (1, new DateOnly(2024, 1, 11), 10m),
            (1, new DateOnly(2024, 1, 12), 0m));

        var positions = InvokeBuildPositions(1, holdings);

        Assert.Single(positions);
        Assert.Equal(new DateOnly(2024, 1, 10), positions[0].OpenDate);
        Assert.Equal(new DateOnly(2024, 1, 12), positions[0].CloseDate);
    }

    [Fact]
    public void BuildPositions_TwoStints_CreatesTwoPositions()
    {
        // Consecutive dates within each stint to avoid gap-based closure
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 5), 10m),
            (1, new DateOnly(2024, 1, 6), 0m),
            (1, new DateOnly(2024, 1, 7), 5m),
            (1, new DateOnly(2024, 1, 8), 5m));

        var positions = InvokeBuildPositions(1, holdings);

        Assert.Equal(2, positions.Count);

        Assert.Equal(new DateOnly(2024, 1, 5), positions[0].OpenDate);
        Assert.NotNull(positions[0].CloseDate);

        Assert.Equal(new DateOnly(2024, 1, 7), positions[1].OpenDate);
        Assert.Null(positions[1].CloseDate);
    }

    [Fact]
    public void BuildPositions_MultipleInstruments_CreatesPositionPerInstrument()
    {
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 10), 10m),
            (1, new DateOnly(2024, 1, 11), 10m),
            (2, new DateOnly(2024, 1, 10), 5m),
            (2, new DateOnly(2024, 1, 11), 5m));

        var positions = InvokeBuildPositions(1, holdings);

        Assert.Equal(2, positions.Count);
        Assert.Contains(positions, p => p.InstrumentId == 1);
        Assert.Contains(positions, p => p.InstrumentId == 2);
    }

    [Fact]
    public void BuildPositions_GapInHoldings_ClosesAndReopens()
    {
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 5), 10m),
            (1, new DateOnly(2024, 1, 6), 10m),
            // Gap — no holding on Jan 7-9
            (1, new DateOnly(2024, 1, 10), 10m),
            (1, new DateOnly(2024, 1, 11), 10m));

        var positions = InvokeBuildPositions(1, holdings);

        Assert.Equal(2, positions.Count);
        Assert.NotNull(positions[0].CloseDate);
        Assert.Null(positions[1].CloseDate);
    }

    [Fact]
    public void BuildPositions_SetsPortfolioId()
    {
        var holdings = CreateHoldingsList(
            (1, new DateOnly(2024, 1, 10), 10m));

        var positions = InvokeBuildPositions(42, holdings);

        Assert.Single(positions);
        Assert.Equal(42, positions[0].PortfolioId);
    }
}
