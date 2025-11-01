using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentValuationConfiguration : ValuationConfiguration<InstrumentValuation>
{
    public override void Configure(EntityTypeBuilder<InstrumentValuation> builder)
    {
        builder.ToTable("InstrumentValuation");
        
        builder.HasOne(iv => iv.Instrument)
            .WithMany()
            .HasForeignKey(iv => iv.InstrumentId);
        
        builder.HasIndex(av => new { av.InstrumentId, av.AsOfDate })
            .IsUnique();
        
        base.Configure(builder);
    }
}