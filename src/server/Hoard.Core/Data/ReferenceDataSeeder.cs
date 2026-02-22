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

        await SeedAlertTypesAsync();
        await SeedDataFeedsAsync();
        await SeedCurrenciesAsync();
        await SeedAssetClassesAsync();
        await SeedAssetSubclassesAsync();
        await SeedInstrumentTypesAsync();
        await SeedTransactionTypesAsync();
        
        await _context.SaveChangesAsync(cancellationToken);
        
        await SeedInstrumentsAsync();

        await _context.SaveChangesAsync(cancellationToken);

        await SeedInstrumentDataFeedsAsync();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reference data seeding complete.");
    }

    private async Task SeedAlertTypesAsync()
    {
        var items = new[]
        {
            new AlertType { Id = AlertType.CloseOfDayPortfolioValuation, Name = "Close of Day Portfolio Valuation" }
        };
        await UpsertAsync(_context.AlertTypes, items, x => x.Id);
    }
    
    private async Task SeedDataFeedsAsync()
    {
        var items = new[]
        {
            new DataFeed { Id = DataFeed.News, Name = "News" },
            new DataFeed { Id = DataFeed.Prices, Name = "Prices" },
            new DataFeed { Id = DataFeed.Quotes, Name = "Quotes" },
        };
        await UpsertAsync(_context.DataFeeds, items, x => x.Id);
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
            new AssetClass { Id = 0, Name = "Unknown", Colour = "#808080" },
            new AssetClass { Id = 1, Name = "Equities", Colour = "#B23A48" },
            new AssetClass { Id = 2, Name = "Bonds", Colour = "#2F6FAB" },
            new AssetClass { Id = 3, Name = "Diversifiers", Colour = "#C48A00" },
            new AssetClass { Id = 4, Name = "Cash", Colour = "#2E8B57" },
            new AssetClass { Id = 5, Name = "FX / Currency", Colour = "#D32F2F" }
        };
        await UpsertAsync(_context.AssetClasses, items, x => x.Id);
    }

    private async Task SeedAssetSubclassesAsync()
    {
        var items = new[]
        {
            new AssetSubclass { Id = 0, AssetClassId = 0, Name = "Unknown", Colour = "#808080" },
            new AssetSubclass { Id = 1, AssetClassId = 1, Name = "UK Large-Cap", Colour = "#7A1E2C" },
            new AssetSubclass { Id = 2, AssetClassId = 1, Name = "Global", Colour = "#B23A48" },
            new AssetSubclass { Id = 3, AssetClassId = 1, Name = "USA", Colour = "#8E2433" },
            new AssetSubclass { Id = 4, AssetClassId = 1, Name = "Europe", Colour = "#A22B3A" },
            new AssetSubclass { Id = 5, AssetClassId = 1, Name = "Emerging Markets", Colour = "#C5535E" },
            new AssetSubclass { Id = 6, AssetClassId = 1, Name = "Japan", Colour = "#D76C74" },
            new AssetSubclass { Id = 7, AssetClassId = 2, Name = "Corporate Bonds", Colour = "#5E97D1" },
            new AssetSubclass { Id = 8, AssetClassId = 2, Name = "Gilts", Colour = "#1F4F7A" },
            new AssetSubclass { Id = 9, AssetClassId = 2, Name = "Global Govt. Bonds", Colour = "#2F6FAB" },
            new AssetSubclass { Id = 10, AssetClassId = 3, Name = "Precious Metals", Colour = "#7A5600" },
            new AssetSubclass { Id = 11, AssetClassId = 3, Name = "Property", Colour = "#936800" },
            new AssetSubclass { Id = 12, AssetClassId = 3, Name = "Commodities", Colour = "#AC7A00" },
            new AssetSubclass { Id = 13, AssetClassId = 3, Name = "Private Equity", Colour = "#D69D1F" },
            new AssetSubclass { Id = 14, AssetClassId = 3, Name = "Infrastructure", Colour = "#C48A00" },
            new AssetSubclass { Id = AssetSubclass.Cash, AssetClassId = 4, Name = "Cash", Colour = "#2E8B57" },
            new AssetSubclass { Id = 16, AssetClassId = 5, Name = "FX / Currency", Colour = "#D81B60" },
            new AssetSubclass { Id = 17, AssetClassId = 1, Name = "UK Mid-Cap", Colour = "#E78C90" },
            new AssetSubclass { Id = 18, AssetClassId = 1, Name = "UK Small-Cap", Colour = "#F2B5B7" },
            new AssetSubclass { Id = 19, AssetClassId = 3, Name = "Multi-Asset", Colour = "#E2B24D" },
            new AssetSubclass { Id = 20, AssetClassId = 3, Name = "Cryptocurrencies", Colour = "#ECC980" },
            new AssetSubclass { Id = 21, AssetClassId = 3, Name = "Derivatives", Colour = "#F6E1B8" },
        };
        await UpsertAsync(_context.AssetSubclasses, items, x => x.Id);
    }
    
    private async Task SeedInstrumentTypesAsync()
    {
        var items = new[]
        {
            new InstrumentType { Id = 0, Name = "Unknown", Colour = "#808080" },
            new InstrumentType { Id = 1, Name = "Share", Colour = "#3B3B3B" },
            new InstrumentType { Id = 2, Name = "ETF", Colour = "#6A4C93" },
            new InstrumentType { Id = 3, Name = "OEIC", Colour = "#1F7A7A" },
            new InstrumentType { Id = 4, Name = "IT", Colour = "#3F51B5" },
            new InstrumentType { Id = 5, Name = "Cash", Colour = "#2E8B57" },
            new InstrumentType { Id = 6, Name = "FX", Colour = "#D32F2F" },
            new InstrumentType { Id = 7, Name = "Gilt", Colour = "#5A6C7D" }
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
            new TransactionType { Id = TransactionType.Promotion, Name = "Promotion" },
            new TransactionType { Id = TransactionType.IncomeDividend, Name = "Dividend" },
        };
        await UpsertAsync(_context.TransactionTypes, items, x => x.Id);
    }
    
    private async Task SeedInstrumentsAsync()
    {
        var items = new[]
        {
            new Instrument { Id = Instrument.Cash, Name = "Cash (GBP)", InstrumentTypeId = InstrumentType.Cash,
                CurrencyId = Currency.Gbp, TickerDisplay = "CASH", AssetSubclassId = 15},

            new Instrument { Id = Instrument.GbpUsd, Name = "GBP/USD", InstrumentTypeId = InstrumentType.FxPair,
                CurrencyId = Currency.Usd, TickerDisplay = "GBPUSD", AssetSubclassId = 16},
            new Instrument { Id = Instrument.GbpEur, Name = "GBP/EUR", InstrumentTypeId = InstrumentType.FxPair,
                CurrencyId = Currency.Eur, TickerDisplay = "GBPEUR", AssetSubclassId = 16},
            new Instrument { Id = Instrument.GbpJpy, Name = "GBP/JPY", InstrumentTypeId = 6,
                CurrencyId = Currency.Jpy, TickerDisplay = "GBPJPY", AssetSubclassId = 16},
            new Instrument { Id = Instrument.GbpDkk, Name = "GBP/DKK", InstrumentTypeId = 6,
                CurrencyId = Currency.Dkk, TickerDisplay = "GBPDKK", AssetSubclassId = 16},
            new Instrument { Id = Instrument.GbpSek, Name = "GBP/SEK", InstrumentTypeId = 6,
                CurrencyId = Currency.Sek, TickerDisplay = "GBPSEK", AssetSubclassId = 16},
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Instrument ON");

        await UpsertAsync(_context.Instruments, items, x => x.Id);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Instrument OFF");
        await transaction.CommitAsync();
    }

    private async Task SeedInstrumentDataFeedsAsync()
    {
        // Define the data feeds for each FX pair instrument
        var feedConfigs = new Dictionary<int, (int feedId, string ticker)[]>
        {
            [Instrument.GbpUsd] = [
                (DataFeed.Prices, "GBPUSD=X"),
                (DataFeed.Quotes, "GBPUSD=X")
            ],
            [Instrument.GbpEur] = [
                (DataFeed.Prices, "GBPEUR=X"),
                (DataFeed.Quotes, "GBPEUR=X")
            ],
            [Instrument.GbpJpy] = [
                (DataFeed.Prices, "GBPJPY=X"),
                (DataFeed.Quotes, "GBPJPY=X")
            ],
            [Instrument.GbpDkk] = [
                (DataFeed.Prices, "GBPDKK=X"),
                (DataFeed.Quotes, "GBPDKK=X")
            ],
            [Instrument.GbpSek] = [
                (DataFeed.Prices, "GBPSEK=X"),
                (DataFeed.Quotes, "GBPSEK=X")
            ]
        };

        // Get existing data feeds to avoid duplicates
        var existingFeeds = await _context.InstrumentDataFeeds
            .Select(idf => new { idf.InstrumentId, idf.DataFeedId })
            .ToListAsync();
        var existingSet = existingFeeds.ToHashSet();

        // Insert missing data feeds
        foreach (var (instrumentId, feeds) in feedConfigs)
        {
            foreach (var (feedId, ticker) in feeds)
            {
                if (!existingSet.Contains(new { InstrumentId = instrumentId, DataFeedId = feedId }))
                {
                    _context.InstrumentDataFeeds.Add(new InstrumentDataFeed
                    {
                        InstrumentId = instrumentId,
                        DataFeedId = feedId,
                        Configuration = new DataFeedConfiguration
                        {
                            Ticker = ticker
                        }
                    });
                }
            }
        }
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