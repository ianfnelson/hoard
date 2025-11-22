using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PerformanceCumulativeConfiguration
{
    public void Configure(EntityTypeBuilder<PerformanceCumulative> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PreviousValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ValueChange)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ValueChangePercent)
            .HasColumnType("decimal(9,4)");

        builder.Property(p => p.UnrealisedGain)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.RealisedGain)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Income)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Return1W).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return1M).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return3M).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return6M).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return1Y).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return3Y).HasColumnType("decimal(9,6)");
        builder.Property(p => p.Return5Y).HasColumnType("decimal(9,6)");
        builder.Property(p => p.ReturnYtd).HasColumnType("decimal(9,6)");
        builder.Property(p => p.ReturnAllTime).HasColumnType("decimal(9,6)");
        builder.Property(p => p.AnnualisedReturn).HasColumnType("decimal(9,6)");

        builder.Property(p => p.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}