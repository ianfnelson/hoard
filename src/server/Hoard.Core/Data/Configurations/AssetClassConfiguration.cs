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
        
        builder.Property(a => a.Code).IsRequired().HasMaxLength(10);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);
        
        builder.HasIndex(a => a.Code).IsUnique();
        
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}