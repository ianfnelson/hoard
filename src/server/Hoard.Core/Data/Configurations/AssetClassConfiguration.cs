using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AssetClassConfiguration : IEntityTypeConfiguration<AssetClass>
{
    public void Configure(EntityTypeBuilder<AssetClass> builder)
    {
        builder.ToTable("AssetClass");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
        
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);
    }
}