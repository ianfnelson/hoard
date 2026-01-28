using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Data;

public class ReferenceDataSeeder
{
    private readonly HoardContext _context;
    private readonly ILogger<ReferenceDataSeeder> _logger;

    public ReferenceDataSeeder(HoardContext context, ILogger<ReferenceDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding reference data...");

        await SeedCurrenciesAsync();
        await SeedAssetClassesAsync();
        await SeedAssetSubclassesAsync();
        await SeedInstrumentTypesAsync();
        await SeedTransactionTypesAsync();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await SeedInstrumentsAsync();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reference data seeding complete.");
    }
    
    private async Task SeedCurrenciesAsync()
    {
        var items = new[]
        {
            new Currency { Id = Currency.Gbp, Name = "Pound Sterling" },
            new Currency { Id = Currency.Gbx, Name = "Pence Sterling" },
            new Currency { Id = Currency.Usd, Name = "US Dollar" },
            new Currency { Id = Currency.Eur, Name = "Euro" },
            new Currency { Id = Currency.Jpy, Name = "Japanese Yen" },
            new Currency { Id = Currency.Dkk, Name = "Danish Krone" },
            new Currency { Id = Currency.Sek, Name = "Swedish Krona" },
        };

        await UpsertAsync(_context.Currencies, items, x => x.Id);
    }
    
    private async Task SeedAssetClassesAsync()
    {
        var items = new[]
        {
            new AssetClass { Id = 0, Code = "UK", Name = "Unknown"},
            new AssetClass { Id = 1, Code = "EQ", Name = "Equities" },
            new AssetClass { Id = 2, Code = "BD", Name = "Bonds and Defensives" },
            new AssetClass { Id = 3, Code = "AL", Name = "Alternatives" },
            new AssetClass { Id = 4, Code = "CA", Name = "Cash" },
            new AssetClass { Id = 5, Code = "FX", Name = "FX / Currency" }
        };
        await UpsertAsync(_context.AssetClasses, items, x => x.Id);
    }

    private async Task SeedAssetSubclassesAsync()
    {
        var items = new[]
        {
            new AssetSubclass { Id = 0, AssetClassId = 0, Code = "UK", Name = "Unknown" },
            new AssetSubclass { Id = 1, AssetClassId = 1, Code = "UKLC", Name = "UK Large-Cap Equities" },
            new AssetSubclass { Id = 2, AssetClassId = 1, Code = "GLEQ", Name = "Global Equities" },
            new AssetSubclass { Id = 3, AssetClassId = 1, Code = "USEQ", Name = "US Equities" },
            new AssetSubclass { Id = 4, AssetClassId = 1, Code = "EUEQ", Name = "European Equities" },
            new AssetSubclass { Id = 5, AssetClassId = 1, Code = "EMEQ", Name = "Emerging Market Equities" },
            new AssetSubclass { Id = 6, AssetClassId = 1, Code = "JPEQ", Name = "Japanese Equities" },
            new AssetSubclass { Id = 7, AssetClassId = 2, Code = "CORP", Name = "Corporate Bonds" },
            new AssetSubclass { Id = 8, AssetClassId = 2, Code = "GILT", Name = "UK Gilts" },
            new AssetSubclass { Id = 9, AssetClassId = 2, Code = "GOVT", Name = "Global Government Bonds" },
            new AssetSubclass { Id = 10, AssetClassId = 2, Code = "GOLD", Name = "Gold and Precious Metals" },
            new AssetSubclass { Id = 11, AssetClassId = 3, Code = "REIT", Name = "Property" },
            new AssetSubclass { Id = 12, AssetClassId = 3, Code = "COMM", Name = "Commodities" },
            new AssetSubclass { Id = 13, AssetClassId = 3, Code = "PREQ", Name = "Private Equity" },
            new AssetSubclass { Id = 14, AssetClassId = 3, Code = "INFR", Name = "Infrastructure" },
            new AssetSubclass { Id = AssetSubclass.Cash, AssetClassId = 4, Code = "CASH", Name = "Cash" },
            new AssetSubclass { Id = 16, AssetClassId = 5, Code = "FX", Name = "FX / Currency" },
            new AssetSubclass { Id = 17, AssetClassId = 1, Code = "UKMC", Name = "UK Mid-Cap Equities" },
            new AssetSubclass { Id = 18, AssetClassId = 1, Code = "UKSC", Name = "UK Small-Cap Equities" },
        };
        await UpsertAsync(_context.AssetSubclasses, items, x => x.Id);
    }
    
    private async Task SeedInstrumentTypesAsync()
    {
        var items = new[]
        {
            new InstrumentType { Id = 0, Code = "UNKNOWN", Name = "Unknown" },
            new InstrumentType { Id = 1, Code = "SHR", Name = "Share" },
            new InstrumentType { Id = 2, Code = "ETF", Name = "Exchange Traded Fund" },
            new InstrumentType { Id = 3, Code = "OEIC", Name = "Open-Ended Investment Company" },
            new InstrumentType { Id = 4, Code = "IT", Name = "Investment Trust" },
            new InstrumentType { Id = 5, Code = "CASH", Name = "Cash", IsCash = true },
            new InstrumentType { Id = 6, Code = "FX", Name = "Foreign Exchange Pair", IsFxPair = true },
            new InstrumentType { Id = 7, Code = "GILT", Name = "Gilts" }
        };
        await UpsertAsync(_context.InstrumentTypes, items, x => x.Id);
    }
    
    private async Task SeedTransactionTypesAsync()
    {
        var items = new[]
        {
            new TransactionType { Id = TransactionType.Buy, Name = "Buy" },
            new TransactionType { Id = TransactionType.Sell, Name = "Sell" },
            new TransactionType { Id = TransactionType.CorporateAction, Name = "Corporate Action" },
            new TransactionType { Id = TransactionType.DepositPersonal, Name = "Personal Contribution" },
            new TransactionType { Id = TransactionType.DepositEmployer, Name = "Employer's Contribution" },
            new TransactionType { Id = TransactionType.DepositTransfer, Name = "Transfer In" },
            new TransactionType { Id = TransactionType.DepositIncomeTaxReclaim, Name = "Income Tax Reclaim" },
            new TransactionType { Id = TransactionType.Withdrawal, Name = "Withdrawal" },
            new TransactionType { Id = TransactionType.Fee, Name = "Fee" },
            new TransactionType { Id = TransactionType.IncomeInterest, Name = "Interest" },
            new TransactionType { Id = TransactionType.IncomeLoyaltyBonus, Name = "Loyalty Bonus" },
            new TransactionType { Id = TransactionType.IncomePromotion, Name = "Promotion" },
            new TransactionType { Id = TransactionType.IncomeDividend, Name = "Dividend" },
        };
        await UpsertAsync(_context.TransactionTypes, items, x => x.Id);
    }
    
    private async Task SeedInstrumentsAsync()
    {
        var items = new[]
        {
            new Instrument { Id = Instrument.Cash, Name = "Cash (GBP)", InstrumentTypeId = InstrumentType.Cash, 
                CurrencyId = Currency.Gbp, EnablePriceUpdates = false,
                TickerDisplay = "CASH", AssetSubclassId = 15},

            new Instrument { Id = Instrument.GbpUsd, Name = "GBP/USD", InstrumentTypeId = InstrumentType.FxPair, 
                CurrencyId = Currency.Usd, TickerDisplay = "GBPUSD", 
                TickerPriceUpdates = "GBPUSD=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpEur, Name = "GBP/EUR", InstrumentTypeId = InstrumentType.FxPair, 
                CurrencyId = Currency.Eur, TickerDisplay = "GBPEUR", 
                TickerPriceUpdates = "GBPEUR=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpJpy, Name = "GBP/JPY", InstrumentTypeId = 6, 
                CurrencyId = Currency.Jpy, TickerDisplay = "GBPJPY", 
                TickerPriceUpdates = "GBPJPY=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpDkk, Name = "GBP/DKK", InstrumentTypeId = 6, 
                CurrencyId = Currency.Dkk, TickerDisplay = "GBPDKK", 
                TickerPriceUpdates = "GBPDKK=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpSek, Name = "GBP/SEK", InstrumentTypeId = 6, 
                CurrencyId = Currency.Sek, TickerDisplay = "GBPSEK", 
                TickerPriceUpdates = "GBPSEK=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Instrument ON");

        await UpsertAsync(_context.Instruments, items, x => x.Id);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Instrument OFF");
        await transaction.CommitAsync();
    }
    
    private static async Task UpsertAsync<TEntity, TKey>(
        DbSet<TEntity> dbSet,
        IEnumerable<TEntity> seedItems,
        Func<TEntity, TKey> keySelector)
        where TEntity : class where TKey : notnull
    {
        var existing = await dbSet.AsNoTracking().ToListAsync();
        var existingMap = existing.ToDictionary(keySelector);

        var toAdd = new List<TEntity>();
        var toUpdate = new List<TEntity>();

        foreach (var item in seedItems)
        {
            var key = keySelector(item);
            if (existingMap.TryGetValue(key, out _))
            {
                // Update the tracked entity
                dbSet.Attach(item);
                dbSet.Entry(item).State = EntityState.Modified;
                toUpdate.Add(item);
            }
            else
            {
                toAdd.Add(item);
            }
        }

        if (toAdd.Any())
            await dbSet.AddRangeAsync(toAdd);
    }
}