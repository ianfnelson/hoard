using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioAssetTargetConfiguration : IEntityTypeConfiguration<PortfolioAssetTarget>
{
    public void Configure(EntityTypeBuilder<PortfolioAssetTarget> builder)
    {
        builder.ToTable("PortfolioAssetTarget", x => 
            x.HasCheckConstraint("CK_Target_0_100", "[Target] BETWEEN 0 AND 100"));

        builder.HasIndex(t => new { t.PortfolioId, t.AssetSubclassId }).IsUnique();
        builder.Property(t => t.Target)
            .HasColumnType("decimal(5,2)")
            .HasPrecision(5,2);
        
        builder.HasOne(t => t.Portfolio)
            .WithMany(p => p.AssetTargets)
            .HasForeignKey(t => t.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssetSubclass)
            .WithMany()
            .HasForeignKey(t => t.AssetSubclassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}