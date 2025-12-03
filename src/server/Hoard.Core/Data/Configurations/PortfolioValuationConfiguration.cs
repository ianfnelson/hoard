using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioValuationConfiguration : ValuationConfiguration<PortfolioValuation>
{
    public override void Configure(EntityTypeBuilder<PortfolioValuation> builder)
    {
        builder.ToTable("PortfolioValuation");

        builder.HasKey(p => p.Id);
        
        builder.HasIndex(pv => new { pv.PortfolioId, pv.AsOfDate })
            .IsUnique();
        
        builder.HasOne(pv => pv.Portfolio)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
    }
}