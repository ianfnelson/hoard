using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class CashflowConfiguration : IEntityTypeConfiguration<Cashflow>
{
    public void Configure(EntityTypeBuilder<Cashflow> builder)
    {
        builder.ToTable("Cashflow");
        
        builder.HasOne(c => c.Transaction)
            .WithMany()
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(c => c.Instrument)
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        
        builder.Property(e => e.Value)
            .HasColumnType("decimal(18, 2)");
        
        builder.HasIndex(c => c.TransactionId).IsUnique();
        builder.HasIndex(c => new { c.AccountId, c.Date });
        builder.HasIndex(c => new { c.InstrumentId, c.Date });
    }
}