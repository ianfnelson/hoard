using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public abstract class ValuationConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Valuation
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(iv => iv.Value)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(e => e.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}