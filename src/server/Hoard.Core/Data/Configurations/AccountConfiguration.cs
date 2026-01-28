using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account");

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasMany(a => a.Portfolios)
            .WithMany(p => p.Accounts)
            .UsingEntity<Dictionary<string, object>>(
                "PortfolioAccount",
                j => j.HasOne<Portfolio>().WithMany()
                    .HasForeignKey("PortfolioId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Account>().WithMany()
                    .HasForeignKey("AccountId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("PortfolioId", "AccountId"));
    }
}