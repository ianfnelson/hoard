using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("Price");

        builder.HasIndex(p => new { p.InstrumentId, p.AsOfDate }).IsUnique();

        builder.Property(p => p.Open).HasColumnType("decimal(18,6)");
        builder.Property(p => p.High).HasColumnType("decimal(18,6)");
        builder.Property(p => p.Low).HasColumnType("decimal(18,6)");
        builder.Property(p => p.Close).HasColumnType("decimal(18,6)");
        builder.Property(p => p.Volume);
        builder.Property(p => p.AdjustedClose).HasColumnType("decimal(18,6)");
        
        builder.Property(p => p.Source).IsRequired().HasMaxLength(50);

        builder.HasOne(p => p.Instrument)
            .WithMany()
            .HasForeignKey(p => p.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}