using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionLegCategoryConfiguration : IEntityTypeConfiguration<TransactionLegCategory>
{
    public void Configure(EntityTypeBuilder<TransactionLegCategory> builder)
    {
        builder.ToTable("TransactionLegCategory");
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
    }
}