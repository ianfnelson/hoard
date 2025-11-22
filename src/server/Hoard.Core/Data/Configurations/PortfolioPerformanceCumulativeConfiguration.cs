using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioPerformanceCumulativeConfiguration 
    : IEntityTypeConfiguration<PortfolioPerformanceCumulative>
{
    public void Configure(EntityTypeBuilder<PortfolioPerformanceCumulative> builder)
    {
        builder.ToTable("PortfolioPerformanceCumulative");

        builder.Property(p => p.PortfolioId)
            .ValueGeneratedNever();

        builder.HasOne(p => p.Portfolio)
            .WithOne()
            .HasForeignKey<PortfolioPerformanceCumulative>(p => p.PortfolioId);
    }
}