using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AssetSubclassConfiguration : IEntityTypeConfiguration<AssetSubclass>
{
    public void Configure(EntityTypeBuilder<AssetSubclass> builder)
    {
        builder.ToTable("AssetSubclass");
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);
        builder.Property(a => a.ShortName).IsRequired().HasMaxLength(10);

        builder.HasOne(a => a.AssetClass)
            .WithMany()
            .HasForeignKey(a => a.AssetClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}