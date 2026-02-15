using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentDataFeedConfiguration : IEntityTypeConfiguration<InstrumentDataFeed>
{
    public void Configure(EntityTypeBuilder<InstrumentDataFeed> builder)
    {
        builder.ToTable("InstrumentDataFeed");

        // Unique constraint: max one feed of each type per instrument
        builder.HasIndex(idf => new { idf.InstrumentId, idf.DataFeedId }).IsUnique();

        builder.Property(e => e.CreatedUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(idf => idf.Instrument)
            .WithMany(i => i.DataFeeds)
            .HasForeignKey(idf => idf.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(idf => idf.DataFeed)
            .WithMany()
            .HasForeignKey(idf => idf.DataFeedId)
            .OnDelete(DeleteBehavior.Restrict);

        // JSON configuration
        builder.OwnsOne(idf => idf.Configuration, conf =>
        {
            conf.ToJson();
            conf.Property(c => c.Ticker)
                .IsRequired()
                .HasMaxLength(20);
        });
    }
}
