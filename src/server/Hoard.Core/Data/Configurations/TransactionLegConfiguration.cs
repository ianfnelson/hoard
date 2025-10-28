using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionLegConfiguration : IEntityTypeConfiguration<TransactionLeg>
{
    public void Configure(EntityTypeBuilder<TransactionLeg> builder)
    {
        builder.ToTable("TransactionLeg");

        builder.Property(l => l.Units).HasColumnType("decimal(18,6)");
        builder.Property(l => l.Value).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.Transaction)
            .WithMany(t => t.Legs)
            .HasForeignKey(l => l.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Instrument)
            .WithMany()
            .HasForeignKey(l => l.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.TransactionLegType)
            .WithMany()
            .HasForeignKey(l => l.TransactionLegTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}