using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Accounts;

public class GetAccountsQuery : IQuery<List<AccountSummaryDto>>
{
    public int? PortfolioId { get; set; }
    public bool? IsActive { get; set; }
}

public class GetAccountsHandler(HoardContext context)
    : IQueryHandler<GetAccountsQuery, List<AccountSummaryDto>>
{
    public async Task<List<AccountSummaryDto>> HandleAsync(GetAccountsQuery query, CancellationToken ct = default)
    {
        var q = context.Accounts.AsNoTracking();

        if (query.IsActive.HasValue)
        {
            q = q.Where(a => a.IsActive == query.IsActive.Value);
        }

        if (query.PortfolioId.HasValue)
        {
            q = q.Where(a => a.Portfolios.Any(p => p.Id == query.PortfolioId.Value));
        }

        var dtos = await q
            .Select(a => new AccountSummaryDto
            {
                Id = a.Id,
                Name = a.Name,
                IsActive = a.IsActive,
                CreatedUtc = a.CreatedUtc,
                AccountTypeId = a.AccountTypeId,
                AccountTypeName = a.AccountType.Name
            })
            .ToListAsync(ct);

        return dtos;
    }
}