using Hoard.Core.Application.Transactions.Models;
using Hoard.Core.Domain;

namespace Hoard.Core.Application.Transactions;

public interface ITransactionFactory
{
    Transaction Create(TransactionWriteDto dto);
}

public class TransactionFactory : ITransactionFactory
{
    public Transaction Create(TransactionWriteDto dto)
    {
        var tx = new Transaction
        {
            AccountId = dto.AccountId!.Value,
            CategoryId = dto.CategoryId!.Value,
            ContractNoteReference = dto.ContractNoteReference,
            Date = dto.Date!.Value,
            InstrumentId = dto.InstrumentId,
            Notes = dto.Notes
        };
        
        foreach (var leg in BuildLegs(dto))
        {
            tx.Legs.Add(leg);
        }

        EnsureBalanced(tx);

        return tx;
    }
    
    private static IEnumerable<TransactionLeg> BuildLegs(TransactionWriteDto dto) =>
        dto.CategoryId switch
        {
            TransactionCategory.Buy            => BuildBuyLegs(dto),
            TransactionCategory.Sell           => BuildSellLegs(dto),
            TransactionCategory.Income         => BuildIncomeLegs(dto),
            TransactionCategory.Fee            => BuildFeeLegs(dto),
            TransactionCategory.Deposit        => BuildDepositLegs(dto),
            TransactionCategory.Withdrawal     => BuildWithdrawalLegs(dto),
            _ => throw new DomainException($"Unhandled category: {dto.CategoryId}")
        };

    private static IEnumerable<TransactionLeg> BuildBuyLegs(TransactionWriteDto dto)
    {
        yield return new TransactionLeg
        {
            InstrumentId = Instrument.CashGbpId,
            CategoryId = TransactionLegCategory.Cash,
            Units = -dto.ValueGbp!.Value,
            ValueGbp = -dto.ValueGbp!.Value
        };

        yield return new TransactionLeg
        {
            InstrumentId = dto.InstrumentId!.Value,
            CategoryId = TransactionLegCategory.Principal,
            Units = dto.Units!.Value,
            ValueGbp = dto.ValueGbp!.Value - (
                (dto.DealingChargeGbp ?? decimal.Zero) +
                (dto.StampDutyGbp ?? decimal.Zero) +
                (dto.FxChargeGbp ?? decimal.Zero) +
                (dto.PtmLevyGbp ?? decimal.Zero)
            )
        };

        foreach (var feeOrTaxLeg in BuildTradeFeeAndTaxLegs(dto))
        {
            yield return feeOrTaxLeg;
        }
    }

    private static IEnumerable<TransactionLeg> BuildTradeFeeAndTaxLegs(TransactionWriteDto dto)
    {
        if (dto.DealingChargeGbp.HasValue)
        {
            yield return new TransactionLeg
            {
                InstrumentId = Instrument.ExternalCashGbpId,
                CategoryId = TransactionLegCategory.Fee,
                Units = dto.DealingChargeGbp.Value,
                ValueGbp = dto.DealingChargeGbp.Value,
                SubcategoryId = TransactionLegSubcategory.DealingCharge
            };
        }

        if (dto.StampDutyGbp.HasValue)
        {
            yield return new TransactionLeg
            {
                InstrumentId = Instrument.ExternalCashGbpId,
                CategoryId = TransactionLegCategory.Tax,
                Units = dto.StampDutyGbp.Value,
                ValueGbp = dto.StampDutyGbp.Value,
                SubcategoryId = TransactionLegSubcategory.StampDuty
            };
        }

        if (dto.FxChargeGbp.HasValue)
        {
            yield return new TransactionLeg
            {
                InstrumentId = Instrument.ExternalCashGbpId,
                CategoryId = TransactionLegCategory.Fee,
                Units = dto.FxChargeGbp.Value,
                ValueGbp = dto.FxChargeGbp.Value,
                SubcategoryId = TransactionLegSubcategory.FxConversionCharge
            };
        }

        if (dto.PtmLevyGbp.HasValue)
        {
            yield return new TransactionLeg
            {
                InstrumentId = Instrument.ExternalCashGbpId,
                CategoryId = TransactionLegCategory.Tax,
                Units = dto.PtmLevyGbp.Value,
                ValueGbp = dto.PtmLevyGbp.Value,
                SubcategoryId = TransactionLegSubcategory.PtmLevy
            };
        }
    }

    private static IEnumerable<TransactionLeg> BuildSellLegs(TransactionWriteDto dto)
    {
        yield return new TransactionLeg
        {
            InstrumentId = Instrument.CashGbpId,
            CategoryId = TransactionLegCategory.Cash,
            Units = dto.ValueGbp!.Value,
            ValueGbp = dto.ValueGbp!.Value
        };

        yield return new TransactionLeg
        {
            InstrumentId = dto.InstrumentId!.Value,
            CategoryId = TransactionLegCategory.Principal,
            Units = -dto.Units!.Value,
            ValueGbp = -dto.ValueGbp!.Value - (
                (dto.DealingChargeGbp ?? decimal.Zero) +
                (dto.StampDutyGbp ?? decimal.Zero) +
                (dto.FxChargeGbp ?? decimal.Zero) +
                (dto.PtmLevyGbp ?? decimal.Zero)
            )
        };

        foreach (var feeOrTaxLeg in BuildTradeFeeAndTaxLegs(dto))
        {
            yield return feeOrTaxLeg;
        }
    }

    private static IEnumerable<TransactionLeg> BuildIncomeLegs(TransactionWriteDto dto)
    {
        return BuildCashAndNonCashPair(dto, TransactionLegCategory.Income, 1);
    }

    private static IEnumerable<TransactionLeg> BuildFeeLegs(TransactionWriteDto dto)
    {
        return BuildCashAndNonCashPair(dto, TransactionLegCategory.Fee, -1);
    }

    private static IEnumerable<TransactionLeg> BuildDepositLegs(TransactionWriteDto dto)
    {
        return BuildCashAndNonCashPair(dto, TransactionLegCategory.External, 1);
    }

    private static IEnumerable<TransactionLeg> BuildWithdrawalLegs(TransactionWriteDto dto)
    {
        return BuildCashAndNonCashPair(dto, TransactionLegCategory.External, -1);
    }
    
    private static IEnumerable<TransactionLeg> BuildCashAndNonCashPair(
        TransactionWriteDto dto, int nonCashCategoryId, int cashSign)
    {
        var amount = dto.ValueGbp!.Value;
        
        yield return new TransactionLeg
        {
            InstrumentId = Instrument.CashGbpId,
            CategoryId = TransactionLegCategory.Cash,
            Units = cashSign * amount,
            ValueGbp = cashSign * amount
        };
        
        yield return new TransactionLeg
        {
            InstrumentId = Instrument.ExternalCashGbpId,
            CategoryId = nonCashCategoryId,
            Units = -cashSign * amount,
            ValueGbp = -cashSign * amount,
            SubcategoryId = dto.NonCashSubcategoryId
        };
    }
    
    private static void EnsureBalanced(Transaction tx)
    {
        var total = tx.Legs.Sum(l => l.ValueGbp);

        if (total != 0)
        {
            throw new DomainException($"Transaction legs do not balance: net = {total}");
        }
    }
}