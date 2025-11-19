using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class HoldingValuationConfiguration : IEntityTypeConfiguration<HoldingValuation>
{
    public void Configure(EntityTypeBuilder<HoldingValuation> builder)
    {
        builder.ToTable("HoldingValuation");

        builder.HasIndex(hv => hv.HoldingId).IsUnique();
        
        builder.HasOne(hv => hv.Holding)
            .WithOne(h => h.Valuation)
            .HasForeignKey<HoldingValuation>(hv => hv.HoldingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(iv => iv.Value)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(e => e.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}