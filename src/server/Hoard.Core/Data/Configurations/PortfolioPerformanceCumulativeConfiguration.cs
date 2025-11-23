using Hoard.Core.Domain;
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

        builder.Property(p => p.PortfolioId)
            .ValueGeneratedNever();

        builder.HasOne(p => p.Portfolio)
            .WithOne()
            .HasForeignKey<PortfolioPerformanceCumulative>(p => p.PortfolioId);
    }
}