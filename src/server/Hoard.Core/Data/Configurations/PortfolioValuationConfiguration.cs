using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioValuationConfiguration : ValuationConfiguration<PortfolioValuation>
{
    public override void Configure(EntityTypeBuilder<PortfolioValuation> builder)
    {
        builder.ToTable("PortfolioValuation");
        
        builder.HasOne(pv => pv.Portfolio)
            .WithMany()
            .HasForeignKey(pv => pv.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(av => new { av.PortfolioId, av.AsOfDate })
            .IsUnique();
        
        base.Configure(builder);
    }
}