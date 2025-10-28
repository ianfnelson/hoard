using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AssetClassConfiguration : IEntityTypeConfiguration<AssetClass>
{
    public void Configure(EntityTypeBuilder<AssetClass> builder)
    {
        builder.ToTable("AssetClass");
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);
        builder.Property(a => a.ShortName).IsRequired().HasMaxLength(10);
    }
}