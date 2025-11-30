using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PositionPerformanceCumulativeConfiguration 
    : PerformanceCumulativeConfiguration<PositionPerformanceCumulative>
{
    public override void Configure(EntityTypeBuilder<PositionPerformanceCumulative> builder)
    {
        builder.ToTable("PositionPerformanceCumulative");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PortfolioPerformanceCumulativeId");
        
        base.Configure(builder);

        builder.HasOne(p => p.Position)
            .WithOne(p => p.Performance)
            .HasForeignKey<PositionPerformanceCumulative>(p => p.PositionId);

        builder.Property(p => p.CostBasis)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Units)
            .HasColumnType("decimal(18,6)");

        builder.Property(p => p.PortfolioWeightPercent)
            .HasColumnType("decimal(9,4)");
    }
}