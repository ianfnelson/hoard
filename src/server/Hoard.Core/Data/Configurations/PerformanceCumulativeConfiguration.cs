using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public abstract class PerformanceCumulativeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : PerformanceCumulative
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(p => p.Value)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PreviousValue)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ValueChange)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.UnrealisedGain)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.RealisedGain)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Income)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.Return1D).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return1W).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return1M).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return3M).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return6M).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return1Y).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return3Y).HasColumnType("decimal(18,4)");
        builder.Property(p => p.Return5Y).HasColumnType("decimal(18,4)");
        builder.Property(p => p.ReturnYtd).HasColumnType("decimal(18,4)");
        builder.Property(p => p.ReturnAllTime).HasColumnType("decimal(18,4)");
        builder.Property(p => p.AnnualisedReturn).HasColumnType("decimal(18,4)");

        builder.Property(p => p.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}