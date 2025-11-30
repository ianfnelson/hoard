using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioPerformanceCumulativeConfiguration 
    : PerformanceCumulativeConfiguration<PortfolioPerformanceCumulative>
{
    public override void Configure(EntityTypeBuilder<PortfolioPerformanceCumulative> builder)
    {
        builder.ToTable("PortfolioPerformanceCumulative");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PortfolioPerformanceCumulativeId");
        
        base.Configure(builder);

        builder.Property(p => p.CashValue)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.CashWeightPercent)
            .HasColumnType("decimal(9,4)");
        
        builder.Property(p => p.PortfolioId)
            .ValueGeneratedNever();

        builder.HasOne(p => p.Portfolio)
            .WithOne(p => p.Performance)
            .HasForeignKey<PortfolioPerformanceCumulative>(p => p.PortfolioId);
    }
}