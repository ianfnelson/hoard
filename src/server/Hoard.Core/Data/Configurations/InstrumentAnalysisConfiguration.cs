using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentAnalysisConfiguration : IEntityTypeConfiguration<InstrumentAnalysis>
{
    public void Configure(EntityTypeBuilder<InstrumentAnalysis> builder)
    {
        builder.ToTable("InstrumentAnalysis");

        builder.HasIndex(a => a.InstrumentId).IsUnique();

        builder.Property(a => a.FiftyTwoWeekHighClose).HasColumnType("decimal(18,4)");
        builder.Property(a => a.FiftyTwoWeekLowClose).HasColumnType("decimal(18,4)");
        builder.Property(a => a.FiftyTwoWeekVolatility).HasColumnType("decimal(18,6)");
        builder.Property(a => a.UpdatedUtc).HasPrecision(3);

        builder.HasOne(a => a.Instrument)
            .WithOne()
            .HasForeignKey<InstrumentAnalysis>(a => a.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
