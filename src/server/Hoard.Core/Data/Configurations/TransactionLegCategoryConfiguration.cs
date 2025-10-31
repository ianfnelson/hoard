using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionLegCategoryConfiguration : IEntityTypeConfiguration<TransactionLegCategory>
{
    public void Configure(EntityTypeBuilder<TransactionLegCategory> builder)
    {
        builder.ToTable("TransactionLegCategory");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
        
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
    }
}