using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Shared;

public static class TestContextFactory
{
    public static HoardContext Create()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HoardContext(options);
    }
}

public static class TestData
{
    public static Account Account(int id = 1, string name = "Test Account") => new()
    {
        Id = id,
        Name = name,
        Colour = "#FF0000"
    };

    public static Instrument Instrument(int id = 1, string name = "Test Stock", string ticker = "TEST") => new()
    {
        Id = id,
        InstrumentTypeId = 1,
        AssetSubclassId = 1,
        Name = name,
        Ticker = ticker,
        CurrencyId = "GBP"
    };

    public static Portfolio Portfolio(int id = 1, string name = "Test Portfolio") => new()
    {
        Id = id,
        Name = name
    };

    public static Transaction BuyTransaction(
        int accountId = 1,
        int? instrumentId = 1,
        DateOnly? date = null,
        decimal? price = 100.50m,
        decimal? units = 10,
        decimal value = -1005m) => new()
    {
        AccountId = accountId,
        InstrumentId = instrumentId,
        TransactionTypeId = Hoard.Core.Domain.Entities.TransactionType.Buy,
        Date = date ?? new DateOnly(2024, 1, 15),
        Price = price,
        Units = units,
        Value = value
    };

    public static Transaction SellTransaction(
        int accountId = 1,
        int? instrumentId = 1,
        DateOnly? date = null,
        decimal? price = 150.75m,
        decimal? units = -10,
        decimal value = 1507.50m) => new()
    {
        AccountId = accountId,
        InstrumentId = instrumentId,
        TransactionTypeId = Hoard.Core.Domain.Entities.TransactionType.Sell,
        Date = date ?? new DateOnly(2024, 1, 15),
        Price = price,
        Units = units,
        Value = value
    };

    public static Price Price(
        int instrumentId = 1,
        DateOnly? asOfDate = null,
        decimal close = 99.00m,
        string source = "Yahoo") => new()
    {
        InstrumentId = instrumentId,
        AsOfDate = asOfDate ?? new DateOnly(2024, 1, 15),
        Close = close,
        AdjustedClose = close,
        Source = source,
        IsLocked = false,
        RetrievedUtc = DateTime.UtcNow
    };

    public static AssetClass AssetClass(int id = 1, string name = "Test Asset Class", string colour = "#0000FF") => new()
    {
        Id = id,
        Name = name,
        Colour = colour
    };

    public static AssetSubclass AssetSubclass(int id = 1, string name = "Test Subclass", int assetClassId = 1, string colour = "#0000AA") => new()
    {
        Id = id,
        Name = name,
        AssetClassId = assetClassId,
        Colour = colour
    };

    public static Currency Currency(string id = "GBP", string name = "British Pound") => new()
    {
        Id = id,
        Name = name
    };

    public static InstrumentType InstrumentType(int id = 1, string name = "Test Type", string colour = "#FF0000") => new()
    {
        Id = id,
        Name = name,
        Colour = colour
    };

    public static TransactionType TransactionType(
        int id = Hoard.Core.Domain.Entities.TransactionType.Buy,
        string name = "Buy") => new()
    {
        Id = id,
        Name = name
    };

    /// <summary>
    /// Populates a context with a coherent, minimal set of reference data:
    /// one asset class, one subclass, one currency (GBP), one instrument type, and one transaction type (Buy).
    /// </summary>
    public static void SeedReferenceData(HoardContext context)
    {
        context.AssetClasses.Add(AssetClass());
        context.AssetSubclasses.Add(AssetSubclass());
        context.Currencies.Add(Currency());
        context.InstrumentTypes.Add(InstrumentType());
        context.TransactionTypes.Add(TransactionType());
    }
}
