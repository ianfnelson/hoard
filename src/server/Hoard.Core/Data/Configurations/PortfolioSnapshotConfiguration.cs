using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class PortfolioSnapshotConfiguration : IEntityTypeConfiguration<PortfolioSnapshot>
{
    public void Configure(EntityTypeBuilder<PortfolioSnapshot> builder)
    {
        builder.ToTable("PortfolioSnapshot");
        builder.HasKey(p => p.Id);
        
        builder.HasOne(p => p.Portfolio)
            .WithMany()
            .HasForeignKey(p => p.PortfolioId);
        
        builder.Property(p => p.StartValue)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.EndValue)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.ValueChange)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.AverageValue)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.Return)
            .HasColumnType("decimal(18,4)");
        
        builder.Property(p => p.Churn)
            .HasColumnType("decimal(18,4)");
        
        builder.Property(p => p.TotalBuys)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalSells)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalIncomeInterest)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalIncomeLoyaltyBonus)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalIncomePromotion)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalIncomeDividends)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalFees)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalDealingCharge)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalStampDuty)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalPtmLevy)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalFxCharge)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalDepositPersonal)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalDepositEmployer)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalDepositIncomeTaxReclaim)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalDepositTransferIn)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.TotalWithdrawals)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(e => e.UpdatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}