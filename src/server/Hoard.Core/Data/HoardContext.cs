using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hoard.Core.Data;

public class HoardContext : DbContext
{
    public HoardContext(DbContextOptions<HoardContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertType> AlertTypes => Set<AlertType>();
    public DbSet<AssetClass> AssetClasses => Set<AssetClass>();
    public DbSet<AssetSubclass> AssetSubclasses => Set<AssetSubclass>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<DataFeed> DataFeeds => Set<DataFeed>();
    public DbSet<Holding> Holdings => Set<Holding>();
    public DbSet<HoldingValuation> HoldingValuations => Set<HoldingValuation>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<InstrumentAnalysis> InstrumentAnalyses => Set<InstrumentAnalysis>();
    public DbSet<InstrumentDataFeed> InstrumentDataFeeds => Set<InstrumentDataFeed>();
    public DbSet<InstrumentType> InstrumentTypes => Set<InstrumentType>();
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<NewsAlertSuppressionRule> NewsAlertSuppressionRules => Set<NewsAlertSuppressionRule>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<TargetAllocation> TargetAllocations => Set<TargetAllocation>();
    public DbSet<PortfolioPerformance> PortfolioPerformances => Set<PortfolioPerformance>();
    public DbSet<PortfolioSnapshot> PortfolioSnapshots => Set<PortfolioSnapshot>();
    public DbSet<PortfolioValuation> PortfolioValuations => Set<PortfolioValuation>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<RunLog> RunLogs => Set<RunLog>();
    public DbSet<RunType> RunTypes => Set<RunType>();
    public DbSet<PositionPerformance> PositionPerformances => Set<PositionPerformance>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var idProp = entity.FindProperty("Id");
            if (idProp != null)
            {
                var newColumnName = entity.ClrType.Name + "Id";
                idProp.SetColumnName(newColumnName);
            }
        }
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HoardContext).Assembly);

        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableUtcConverter);
            }
        }
    }
}