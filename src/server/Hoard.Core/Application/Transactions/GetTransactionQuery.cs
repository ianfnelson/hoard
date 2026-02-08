using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Transactions;

public record GetTransactionQuery(int TransactionId) : IQuery<TransactionDetailDto?>;

public sealed class GetTransactionHandler(HoardContext context, ILogger<GetTransactionHandler> logger)
    : IQueryHandler<GetTransactionQuery, TransactionDetailDto?>
{
    public async Task<TransactionDetailDto?> HandleAsync(GetTransactionQuery query, CancellationToken ct = default)
    {
        var dto = await context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == query.TransactionId)
            .Select(t => new TransactionDetailDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                AccountName = t.Account.Name,
                InstrumentId = t.InstrumentId,
                InstrumentName = t.Instrument == null ? null : t.Instrument.Name,
                InstrumentTicker = t.Instrument == null ? null : t.Instrument.TickerDisplay,
                ContractNoteReference = t.ContractNoteReference,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType.Name,
                Date = t.Date,
                Notes = t.Notes,
                Units = t.Units,
                Value = t.Value,
                Price = t.Price,
                DealingCharge = t.DealingCharge,
                StampDuty = t.StampDuty,
                PtmLevy = t.PtmLevy,
                FxCharge = t.FxCharge
            }).SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "Transaction with id {TransactionId} not found",
                query.TransactionId);
        }

        return dto;
    }
}