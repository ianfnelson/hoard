using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class InstrumentTypeConfiguration : IEntityTypeConfiguration<InstrumentType>
{
    public void Configure(EntityTypeBuilder<InstrumentType> builder)
    {
        builder.ToTable("InstrumentType");

        builder.Property(t => t.Code).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
    }
}