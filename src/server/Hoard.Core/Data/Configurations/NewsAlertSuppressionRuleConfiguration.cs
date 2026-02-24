using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class NewsAlertSuppressionRuleConfiguration : IEntityTypeConfiguration<NewsAlertSuppressionRule>
{
    public void Configure(EntityTypeBuilder<NewsAlertSuppressionRule> builder)
    {
        builder.ToTable("NewsAlertSuppressionRule");
        builder.Property(p => p.HeadlineContains).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}