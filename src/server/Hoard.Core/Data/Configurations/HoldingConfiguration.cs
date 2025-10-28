using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class HoldingConfiguration : IEntityTypeConfiguration<Holding>
{
    public void Configure(EntityTypeBuilder<Holding> builder)
    {
        builder.ToTable("Holding");

        builder.HasIndex(h => new { h.AccountId, h.InstrumentId, h.AsOfDate })
            .IsUnique();

        builder.Property(h => h.Units).HasColumnType("decimal(18,6)");

        builder.HasOne(h => h.Instrument)
            .WithMany()
            .HasForeignKey(h => h.InstrumentId);

        builder.HasOne(h => h.Account)
            .WithMany()
            .HasForeignKey(h => h.AccountId);
    }
}