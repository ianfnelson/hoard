using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionLegSubcategoryConfiguration : IEntityTypeConfiguration<TransactionLegSubcategory>
{
    public void Configure(EntityTypeBuilder<TransactionLegSubcategory> builder)
    {
        builder.ToTable("TransactionLegSubcategory");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
        
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}