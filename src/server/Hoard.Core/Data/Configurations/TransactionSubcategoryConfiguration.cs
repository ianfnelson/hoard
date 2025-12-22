using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class TransactionSubcategoryConfiguration : IEntityTypeConfiguration<TransactionSubcategory>
{
    public void Configure(EntityTypeBuilder<TransactionSubcategory> builder)
    {
        builder.ToTable("TransactionSubcategory");
        
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();
        
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        
        builder.HasOne(x => x.TransactionCategory)
            .WithMany()
            .HasForeignKey("TransactionCategoryId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}