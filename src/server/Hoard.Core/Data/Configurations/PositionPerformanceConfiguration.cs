using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PositionPerformanceConfiguration 
    : PerformanceConfiguration<PositionPerformance>
{
    public override void Configure(EntityTypeBuilder<PositionPerformance> builder)
    {
        builder.ToTable("PositionPerformance");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PositionPerformanceId");
        
        base.Configure(builder);

        builder.HasOne(p => p.Position)
            .WithOne(p => p.Performance)
            .HasForeignKey<PositionPerformance>(p => p.PositionId);

        builder.Property(p => p.CostBasis)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Units)
            .HasColumnType("decimal(18,6)");
    }
}