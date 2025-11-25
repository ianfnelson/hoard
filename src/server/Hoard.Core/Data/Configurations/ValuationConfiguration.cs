using Hoard.Core.Domain;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class ValuationConfiguration : IEntityTypeConfiguration<Valuation>
{
    public void Configure(EntityTypeBuilder<Valuation> builder)
    {
        builder.ToTable("Valuation");

        builder.HasIndex(hv => hv.HoldingId).IsUnique();
        
        builder.HasOne(hv => hv.Holding)
            .WithOne(h => h.Valuation)
            .HasForeignKey<Valuation>(hv => hv.HoldingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(iv => iv.Value)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(e => e.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}