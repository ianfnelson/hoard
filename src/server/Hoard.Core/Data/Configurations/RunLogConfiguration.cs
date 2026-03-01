using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class RunLogConfiguration : IEntityTypeConfiguration<RunLog>
{
    public void Configure(EntityTypeBuilder<RunLog> builder)
    {
        builder.ToTable("RunLog");

        builder.Property(r => r.StartedAtUtc)
            .HasColumnType("datetime2(3)");

        builder.Property(r => r.CompletedAtUtc)
            .HasColumnType("datetime2(3)");

        builder.HasOne(r => r.RunType)
            .WithMany()
            .HasForeignKey(r => r.RunTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.RunId)
            .IsUnique();

        builder.HasIndex(r => r.RunTypeId);
    }
}
