using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class HoldingValuationConfiguration : ValuationConfiguration<HoldingValuation>
{
    public override void Configure(EntityTypeBuilder<HoldingValuation> builder)
    {
        builder.ToTable("HoldingValuation");

        builder.HasKey(p => p.Id);
        
        builder.HasIndex(hv => hv.HoldingId).IsUnique();
        
        builder.HasOne(hv => hv.Holding)
            .WithOne(h => h.Valuation)
            .HasForeignKey<HoldingValuation>(hv => hv.HoldingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}