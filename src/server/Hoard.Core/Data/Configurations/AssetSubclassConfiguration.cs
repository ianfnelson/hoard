using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AssetSubclassConfiguration : IEntityTypeConfiguration<AssetSubclass>
{
    public void Configure(EntityTypeBuilder<AssetSubclass> builder)
    {
        builder.ToTable("AssetSubclass");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
        
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);

        builder.HasOne(a => a.AssetClass)
            .WithMany(a => a.Subclasses)
            .HasForeignKey(a => a.AssetClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}