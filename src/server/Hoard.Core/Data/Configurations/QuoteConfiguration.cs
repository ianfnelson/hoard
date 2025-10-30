using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quote");

        builder.HasIndex(q => q.InstrumentId).IsUnique();
        
        builder.Property(q => q.Bid).HasColumnType("decimal(18,6)");
        builder.Property(q => q.Ask).HasColumnType("decimal(18,6)");
        builder.Property(q => q.FiftyTwoWeekHigh).HasColumnType("decimal(18,6)");
        builder.Property(q => q.FiftyTwoWeekLow).HasColumnType("decimal(18,6)");
        builder.Property(q => q.RegularMarketPrice).HasColumnType("decimal(18,6)");
        builder.Property(q => q.RegularMarketChange).HasColumnType("decimal(18,6)");
        builder.Property(q => q.RegularMarketChangePercent).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Source).IsRequired().HasMaxLength(50);
        
        builder.HasOne(q => q.Instrument)
            .WithOne()
            .HasForeignKey<Quote>(q => q.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}