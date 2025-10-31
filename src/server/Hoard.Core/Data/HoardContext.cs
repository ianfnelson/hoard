using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Data;

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
    public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();
    public DbSet<TransactionLegCategory> TransactionLegCategories => Set<TransactionLegCategory>();
    public DbSet<TransactionLegSubcategory> TransactionLegSubcategories => Set<TransactionLegSubcategory>();
    
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
    }
}