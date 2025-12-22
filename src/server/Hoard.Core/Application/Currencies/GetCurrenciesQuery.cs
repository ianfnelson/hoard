using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Currencies;

public record GetCurrenciesQuery : IQuery<List<CurrencyDto>>;

public class GetCurrenciesHandler(HoardContext context)
    : IQueryHandler<GetCurrenciesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> HandleAsync(GetCurrenciesQuery query, CancellationToken ct = default)
    {
        var dtos = await context.Currencies
            .AsNoTracking()
            .Select(c => new CurrencyDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return dtos;
    }
}