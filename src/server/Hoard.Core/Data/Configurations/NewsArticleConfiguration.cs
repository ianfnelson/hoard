using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("NewsArticle");

        builder.HasOne(na => na.Instrument)
            .WithMany()
            .HasForeignKey(na => na.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(na => na.PublishedUtc)
            .IsRequired()
            .HasColumnType("datetime2(0)");

        builder.Property(na => na.RetrievedUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(p => p.Source)
            .IsRequired().HasMaxLength(50);

        builder.Property(p => p.SourceArticleId)
            .IsRequired().HasMaxLength(50);

        builder.Property(p => p.Url)
            .IsRequired().HasMaxLength(255);

        // Unique constraint to prevent duplicate articles
        builder.HasIndex(na => new { na.Source, na.SourceArticleId })
            .IsUnique();
    }
}