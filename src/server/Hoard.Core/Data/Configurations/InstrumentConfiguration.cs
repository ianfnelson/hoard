using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instrument");

        builder.Property(i => i.Name).IsRequired().HasMaxLength(100);
        builder.Property(i => i.TickerApi).HasMaxLength(20);
        builder.Property(i => i.Ticker).IsRequired().HasMaxLength(20);
        builder.Property(i => i.EnablePriceUpdates).IsRequired();

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
        
        builder.HasOne(i => i.InstrumentType)
            .WithMany()
            .HasForeignKey(i => i.InstrumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.BaseCurrency)
            .WithMany()
            .HasForeignKey(i => i.BaseCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.QuoteCurrency)
            .WithMany()
            .HasForeignKey(i => i.QuoteCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}