using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transaction");

        builder.Property(t => t.Date).IsRequired();
        builder.Property(i => i.ContractNoteReference).HasMaxLength(20);
        
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.DealingCharge)
            .HasColumnType("decimal(18, 2)");
        
        builder.Property(e => e.FxCharge)
            .HasColumnType("decimal(18, 2)");
        
        builder.Property(e => e.StampDuty)
            .HasColumnType("decimal(18, 2)");
        
        builder.Property(e => e.PtmLevy)
            .HasColumnType("decimal(18, 2)");
        
        builder.Property(e => e.Value)
            .HasColumnType("decimal(18, 2)");
        
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18, 4)");
        
        builder.Property(e => e.Units)
            .HasColumnType("decimal(18, 6)");

        builder.Property(e => e.ContractNoteReference)
            .HasMaxLength(20);

        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Instrument)
            .WithMany()
            .HasForeignKey(t => t.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(t => t.TransactionType)
            .WithMany()
            .HasForeignKey(t => t.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.Date);
    }
}