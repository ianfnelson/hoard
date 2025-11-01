using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Data;

public class ReferenceDataSeeder
{
    private readonly HoardContext _db;
    private readonly ILogger<ReferenceDataSeeder> _logger;

    public ReferenceDataSeeder(HoardContext db, ILogger<ReferenceDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding reference data...");

        await SeedAccountTypesAsync();
        await SeedCurrenciesAsync();
        await SeedAssetClassesAsync();
        await SeedAssetSubclassesAsync();
        await SeedInstrumentTypesAsync();
        await SeedTransactionCategoriesAsync();
        await SeedTransactionLegCategoriesAsync();
        await SeedTransactionLegSubCategoriesAsync();
        await SeedInstrumentsAsync();

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reference data seeding complete.");
    }


    private async Task SeedAccountTypesAsync()
    {
        var items = new[]
        {
            new AccountType { Id = 0, Name = "Unknown" },
            new AccountType { Id = 1, Name = "SIPP" },
            new AccountType { Id = 2, Name = "ISA" },
            new AccountType { Id = 3, Name = "GIA" }
        };

        await UpsertAsync(_db.AccountTypes, items, x => x.Id);
    }
    
    private async Task SeedCurrenciesAsync()
    {
        var items = new[]
        {
            new Currency { Id = "GBP", Name = "Pound Sterling" },
            new Currency { Id = "GBp", Name = "Pence Sterling" },
            new Currency { Id = "USD", Name = "US Dollar" },
            new Currency { Id = "EUR", Name = "Euro" },
            new Currency { Id = "JPY", Name = "Japanese Yen" }
        };

        await UpsertAsync(_db.Currencies, items, x => x.Id);
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
        await UpsertAsync(_db.AssetClasses, items, x => x.Id);
    }

    private async Task SeedAssetSubclassesAsync()
    {
        var items = new[]
        {
            new AssetSubclass { Id = 0, AssetClassId = 0, Code = "UK", Name = "Unknown" },
            new AssetSubclass { Id = 1, AssetClassId = 1, Code = "UKEQ", Name = "UK Equities" },
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
            new AssetSubclass { Id = 15, AssetClassId = 4, Code = "CASH", Name = "Cash" },
            new AssetSubclass { Id = 16, AssetClassId = 5, Code = "FX", Name = "FX / Currency" }
        };
        await UpsertAsync(_db.AssetSubclasses, items, x => x.Id);
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
            new InstrumentType { Id = 6, Code = "EXT", Name = "External Cash", IsCash = true, IsExternal = true },
            new InstrumentType { Id = 7, Code = "FX", Name = "Foreign Exchange Pair", IsFxPair = true }
        };
        await UpsertAsync(_db.InstrumentTypes, items, x => x.Id);
    }
    
    private async Task SeedTransactionCategoriesAsync()
    {
        var items = new[]
        {
            new TransactionCategory { Id = 1, Name = "Buy" },
            new TransactionCategory { Id = 2, Name = "Sell" },
            new TransactionCategory { Id = 3, Name = "Dividend" },
            new TransactionCategory { Id = 4, Name = "Interest" },
            new TransactionCategory { Id = 5, Name = "Fee" },
            new TransactionCategory { Id = 6, Name = "Deposit" },
            new TransactionCategory { Id = 7, Name = "Withdrawal" }
        };
        await UpsertAsync(_db.TransactionCategories, items, x => x.Id);
    }
    
    private async Task SeedTransactionLegCategoriesAsync()
    {
        var items = new[] 
        {
            new TransactionLegCategory { Id = 0, Name = "Cash" },
            new TransactionLegCategory { Id = 1, Name = "Principal" },
            new TransactionLegCategory { Id = 2, Name = "Income" },
            new TransactionLegCategory { Id = 3, Name = "Fee" },
            new TransactionLegCategory { Id = 4, Name = "Tax" },
            new TransactionLegCategory { Id = 5, Name = "External" }
        };
        await UpsertAsync(_db.TransactionLegCategories, items, x => x.Id);
    }
    
    private async Task SeedTransactionLegSubCategoriesAsync()
    {
        var items = new[]
        {
            new TransactionLegSubcategory { Id = 1, CategoryId = 5, Name = "Personal Contribution" },
            new TransactionLegSubcategory { Id = 2, CategoryId = 5, Name = "Employer Contribution" },
            new TransactionLegSubcategory { Id = 3, CategoryId = 5, Name = "Income Tax Reclaim" },
            new TransactionLegSubcategory { Id = 4, CategoryId = 5, Name = "Transfer In" },

            new TransactionLegSubcategory { Id = 5, CategoryId = 3, Name = "Account Charge" },
            new TransactionLegSubcategory { Id = 6, CategoryId = 3, Name = "Dealing Charge" },
            new TransactionLegSubcategory { Id = 7, CategoryId = 3, Name = "FX Conversion Charge" },

            new TransactionLegSubcategory { Id = 8, CategoryId = 4, Name = "Stamp Duty" },
            new TransactionLegSubcategory { Id = 9, CategoryId = 4, Name = "PTM Levy" },
        };
        await UpsertAsync(_db.TransactionLegSubcategories, items, x => x.Id);
    }
    
    private async Task SeedInstrumentsAsync()
    {
        var cashSubclassId = await _db.AssetSubclasses
            .Where(s => s.Code == "CASH")
            .Select(s => s.Id)
            .FirstAsync();
        
        var fxSubclassId = await _db.AssetSubclasses
            .Where(s => s.Code == "FX")
            .Select(s => s.Id)
            .FirstAsync();

        var items = new[]
        {
            new Instrument { Id = 1, Name = "Cash (GBP)", InstrumentTypeId = cashSubclassId, 
                BaseCurrencyId = "GBP", QuoteCurrencyId = "GBP", EnablePriceUpdates = false,
                Ticker = "CASH" },
            new Instrument { Id = 2, Name = "External Cash (GBP)", InstrumentTypeId = cashSubclassId, 
                BaseCurrencyId = "GBP", QuoteCurrencyId = "GBP", EnablePriceUpdates = false, 
                Ticker = "EXTERNAL" },

            new Instrument { Id = 10, Name = "GBP/USD", InstrumentTypeId = fxSubclassId, 
                BaseCurrencyId = "GBP", QuoteCurrencyId = "USD", Ticker = "GBPUSD", 
                TickerApi = "GBPUSD=X", EnablePriceUpdates = true },
            new Instrument { Id = 11, Name = "GBP/EUR", InstrumentTypeId = fxSubclassId, 
                BaseCurrencyId = "GBP", QuoteCurrencyId = "EUR", Ticker = "GBPEUR", 
                TickerApi = "GBPEUR=X", EnablePriceUpdates = true },
            new Instrument { Id = 12, Name = "GBP/JPY", InstrumentTypeId = fxSubclassId, 
                BaseCurrencyId = "GBP", QuoteCurrencyId = "JPY", Ticker = "GBPJPY", 
                TickerApi = "GBPJPY=X", EnablePriceUpdates = true }
        };

        await UpsertAsync(_db.Instruments, items, x => x.Id);
    }
    
    private static async Task UpsertAsync<TEntity, TKey>(
        DbSet<TEntity> dbSet,
        IEnumerable<TEntity> seedItems,
        Func<TEntity, TKey> keySelector)
        where TEntity : class
    {
        var existing = await dbSet.AsNoTracking().ToListAsync();
        var keySet = existing.Select(keySelector).ToHashSet();
        var newItems = seedItems.Where(item => !keySet.Contains(keySelector(item))).ToList();

        if (newItems.Any())
            await dbSet.AddRangeAsync(newItems);
    }
}