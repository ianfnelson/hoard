using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Data;

public class HoardContext : DbContext
{
    public HoardContext(DbContextOptions<HoardContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<AssetClass> AssetClasses => Set<AssetClass>();
    public DbSet<AssetSubclass> AssetSubclasses => Set<AssetSubclass>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Holding> Holdings => Set<Holding>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<InstrumentType> InstrumentTypes => Set<InstrumentType>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PortfolioAssetTarget> PortfolioAssetTargets => Set<PortfolioAssetTarget>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionLeg> TransactionLegs => Set<TransactionLeg>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<TransactionLegType> TransactionLegTypes => Set<TransactionLegType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HoardContext).Assembly);
    }
}