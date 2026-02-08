using Hoard.Core.Domain.Entities;

namespace Hoard.Core.Application.Transactions;

public class TransactionMapper
    : IMapper<TransactionWriteDto, Transaction>
{
    public Transaction Map(TransactionWriteDto source)
    {
        var tx = new Transaction();
        Map(source, tx);
        return tx;
    }

    public void Map(TransactionWriteDto source, Transaction destination)
    {
        destination.AccountId = source.AccountId!.Value;
        destination.TransactionTypeId = source.TransactionTypeId!.Value;
        destination.ContractNoteReference = source.ContractNoteReference;
        destination.Date = source.Date!.Value;
        destination.DealingCharge = source.DealingCharge;
        destination.FxCharge = source.FxCharge;
        destination.InstrumentId = source.InstrumentId;
        destination.Notes = source.Notes;
        destination.Price = source.Price;
        destination.PtmLevy = source.PtmLevy;
        destination.StampDuty = source.StampDuty;
        destination.Units = MapUnits(source);
        destination.Value = MapValue(source);
    }

    private static decimal? MapUnits(TransactionWriteDto source)
    {
        return TransactionTypeSets.NegativeUnits.Contains(source.TransactionTypeId!.Value) ? -source.Units : source.Units;
    }
    
    private static decimal MapValue(TransactionWriteDto source)
    {
        return TransactionTypeSets.NegativeValue.Contains(source.TransactionTypeId!.Value) ? -source.Value!.Value : source.Value!.Value;
    }
}