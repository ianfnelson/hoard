using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alert");
        
        builder.Property(a => a.CreatedUtc)
            .HasColumnType("datetime2(3)");

        builder.Property(a => a.SuppressionKey)
            .HasMaxLength(100);
        
        builder.Property(a => a.Title)
            .HasMaxLength(200);
        
        builder.Property(a => a.Url)
            .HasMaxLength(200);
        
        builder.Property(a => a.UrlTitle)
            .HasMaxLength(200);
        
        builder.HasOne(a => a.Portfolio)
            .WithMany()
            .HasForeignKey(a => a.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.Instrument)
            .WithMany()
            .HasForeignKey(a => a.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(a => a.AlertType)
            .WithMany()
            .HasForeignKey(a => a.AlertTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(a => a.CreatedUtc);
        builder.HasIndex(a => a.PortfolioId);
        builder.HasIndex(a => a.InstrumentId);
        builder.HasIndex(a => a.AlertTypeId);
        builder.HasIndex(a => a.SuppressionKey)
            .IsUnique();
    }
}