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

        await SeedAccountTypesAsync();
        await SeedCurrenciesAsync();
        await SeedAssetClassesAsync();
        await SeedAssetSubclassesAsync();
        await SeedInstrumentTypesAsync();
        await SeedTransactionCategoriesAsync();
        await SeedTransactionSubcategoriesAsync();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await SeedInstrumentsAsync();

        await _context.SaveChangesAsync(cancellationToken);

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

        await UpsertAsync(_context.AccountTypes, items, x => x.Id);
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
    
    private async Task SeedTransactionCategoriesAsync()
    {
        var items = new[]
        {
            new TransactionCategory { Id = TransactionCategory.Buy, Name = "Buy" },
            new TransactionCategory { Id = TransactionCategory.Sell, Name = "Sell" },
            new TransactionCategory { Id = TransactionCategory.Income, Name = "Income" },
            new TransactionCategory { Id = TransactionCategory.Fee, Name = "Fee" },
            new TransactionCategory { Id = TransactionCategory.Deposit, Name = "Deposit" },
            new TransactionCategory { Id = TransactionCategory.Withdrawal, Name = "Withdrawal" },
            new TransactionCategory { Id = TransactionCategory.CorporateAction, Name = "Corporate Action" }
        };
        await UpsertAsync(_context.TransactionCategories, items, x => x.Id);
    }
    
    private async Task SeedTransactionSubcategoriesAsync()
    {
        var items = new[]
        {
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.PersonalContribution, 
                Name = "Personal Contribution", 
                TransactionCategoryId = TransactionCategory.Deposit
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.EmployerContribution, 
                Name = "Employer Contribution", 
                TransactionCategoryId = TransactionCategory.Deposit
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.IncomeTaxReclaim, 
                Name = "Income Tax Reclaim", 
                TransactionCategoryId = TransactionCategory.Deposit
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.TransferIn, 
                Name = "Transfer In", 
                TransactionCategoryId = TransactionCategory.Deposit
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.Interest, 
                Name = "Interest", 
                TransactionCategoryId = TransactionCategory.Income
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.LoyaltyBonus, 
                Name = "Loyalty Bonus", 
                TransactionCategoryId = TransactionCategory.Income
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.Promotion, 
                Name = "Promotion", 
                TransactionCategoryId = TransactionCategory.Income
            },
            new TransactionSubcategory
            {
                Id = TransactionSubcategory.Dividend, 
                Name = "Dividend / Tax Credit", 
                TransactionCategoryId = TransactionCategory.Income
            }
        };
        await UpsertAsync(_context.TransactionSubcategories, items, x => x.Id);
    }
    
    private async Task SeedInstrumentsAsync()
    {
        var items = new[]
        {
            new Instrument { Id = Instrument.Cash, Name = "Cash (GBP)", InstrumentTypeId = InstrumentType.Cash, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Gbp, EnablePriceUpdates = false,
                Ticker = "CASH", AssetSubclassId = 15},

            new Instrument { Id = Instrument.GbpUsd, Name = "GBP/USD", InstrumentTypeId = InstrumentType.FxPair, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Usd, Ticker = "GBPUSD", 
                TickerApi = "GBPUSD=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpEur, Name = "GBP/EUR", InstrumentTypeId = InstrumentType.FxPair, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Eur, Ticker = "GBPEUR", 
                TickerApi = "GBPEUR=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpJpy, Name = "GBP/JPY", InstrumentTypeId = 6, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Jpy, Ticker = "GBPJPY", 
                TickerApi = "GBPJPY=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpDkk, Name = "GBP/DKK", InstrumentTypeId = 6, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Dkk, Ticker = "GBPDKK", 
                TickerApi = "GBPDKK=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
            new Instrument { Id = Instrument.GbpSek, Name = "GBP/SEK", InstrumentTypeId = 6, 
                BaseCurrencyId = Currency.Gbp, QuoteCurrencyId = Currency.Sek, Ticker = "GBPSEK", 
                TickerApi = "GBPSEK=X", EnablePriceUpdates = true, AssetSubclassId = 16 },
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
            if (existingMap.TryGetValue(key, out var existingItem))
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