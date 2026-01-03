using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioPerformanceConfiguration 
    : PerformanceConfiguration<PortfolioPerformance>
{
    public override void Configure(EntityTypeBuilder<PortfolioPerformance> builder)
    {
        builder.ToTable("PortfolioPerformance");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("PortfolioPerformanceId");
        
        base.Configure(builder);

        builder.Property(p => p.CashValue)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.PortfolioId)
            .ValueGeneratedNever();

        builder.HasOne(p => p.Portfolio)
            .WithOne(p => p.Performance)
            .HasForeignKey<PortfolioPerformance>(p => p.PortfolioId);
    }
}