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
        TransactionTypeId = TransactionType.Buy,
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
        TransactionTypeId = TransactionType.Sell,
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
}
