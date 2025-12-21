using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Currencies;

public record GetCurrencyQuery(string CurrencyId) : IQuery<CurrencyDto?>;

public class GetCurrencyHandler(HoardContext context, ILogger<GetCurrencyHandler> logger)
    : IQueryHandler<GetCurrencyQuery, CurrencyDto?>
{
    public async Task<CurrencyDto?> HandleAsync(GetCurrencyQuery query, CancellationToken ct = default)
    {
        var dto = await context.Currencies
            .AsNoTracking()
            .Where(c => c.Id == query.CurrencyId)
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedUtc = c.CreatedUtc
            })
            .SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "Currency with id {CurrencyId} not found", query.CurrencyId);
        }

        return dto;
    }
}