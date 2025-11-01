using Hoard.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hoard.Core.Data.Configurations;

public class AccountValuationConfiguration : ValuationConfiguration<AccountValuation>
{
    public override void Configure(EntityTypeBuilder<AccountValuation> builder)
    {
        builder.ToTable("AccountValuation");

        builder.HasOne(av => av.Account)
            .WithMany()
            .HasForeignKey(av => av.AccountId);
        
        builder.HasIndex(av => new { av.AccountId, av.AsOfDate })
            .IsUnique();
        
        base.Configure(builder);
    }
}