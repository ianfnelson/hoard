using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionLegTypeConfiguration : IEntityTypeConfiguration<TransactionLegType>
{
    public void Configure(EntityTypeBuilder<TransactionLegType> builder)
    {
        builder.ToTable("TransactionLegType");
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
    }
}