using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PositionPerformanceCumulativeConfiguration 
    : IEntityTypeConfiguration<PositionPerformanceCumulative>
{
    public void Configure(EntityTypeBuilder<PositionPerformanceCumulative> builder)
    {
        builder.ToTable("PositionPerformanceCumulative");

        builder.HasOne(p => p.Position)
            .WithOne()
            .HasForeignKey<PositionPerformanceCumulative>(p => p.PositionId);

        builder.Property(p => p.CostBasis)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Units)
            .HasColumnType("decimal(18,6)");

        builder.Property(p => p.PortfolioWeightPercent)
            .HasColumnType("decimal(9,4)");
    }
}