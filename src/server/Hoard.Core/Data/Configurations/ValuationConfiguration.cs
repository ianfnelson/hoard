using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class ValuationConfiguration<TValuation> : IEntityTypeConfiguration<TValuation> where TValuation : Valuation
{
    public virtual void Configure(EntityTypeBuilder<TValuation> builder)
    {
        builder.Property(iv => iv.Value)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(e => e.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}