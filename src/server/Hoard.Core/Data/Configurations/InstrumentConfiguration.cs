using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instrument");

        builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
        builder.Property(i => i.TickerNewsUpdates).HasMaxLength(20);
        builder.Property(i => i.TickerPriceUpdates).HasMaxLength(20);
        builder.Property(i => i.TickerDisplay).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Isin).HasColumnType("char(12)");
        builder.Property(i => i.EnablePriceUpdates).IsRequired().HasDefaultValue(false);
        builder.Property(i => i.EnableNewsUpdates).IsRequired().HasDefaultValue(false);

        builder.Property(i => i.AssetSubclassId)
            .HasDefaultValue(0)
            .IsRequired();
        
        builder.HasOne(i => i.AssetSubclass)
            .WithMany()
            .HasForeignKey(i => i.AssetSubclassId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(i => i.InstrumentTypeId)
            .HasDefaultValue(0)
            .IsRequired();
        
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        
        builder.HasOne(i => i.InstrumentType)
            .WithMany()
            .HasForeignKey(i => i.InstrumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Currency)
            .WithMany()
            .HasForeignKey(i => i.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(i => i.Isin).IsUnique();
    }
}