using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.AccountTypes;

public record GetAccountTypesQuery : IQuery<List<AccountTypeDto>>;

public class GetAccountTypesHandler(HoardContext context)
    : IQueryHandler<GetAccountTypesQuery, List<AccountTypeDto>>
{
    public async Task<List<AccountTypeDto>> HandleAsync(GetAccountTypesQuery query, CancellationToken ct = default)
    {
        var dtos = await context.AccountTypes
            .AsNoTracking()
            .Select(at => new AccountTypeDto
            {
                Id = at.Id,
                Name = at.Name,
                CreatedUtc = at.CreatedUtc,
            })
            .OrderBy(at => at.Name)
            .ToListAsync(ct);

        return dtos;
    }
}