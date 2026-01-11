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
        destination.PtmLevy = source.PtmLevy;
        destination.StampDuty = source.StampDuty;
        destination.Units = source.Units;
        destination.Value = source.Value!.Value;
    }
}