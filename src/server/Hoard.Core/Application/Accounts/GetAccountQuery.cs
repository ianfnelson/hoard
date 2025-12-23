using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.Accounts;

public record GetAccountQuery(int AccountId) : IQuery<AccountDetailDto?>;

public class GetAccountHandler(HoardContext context, ILogger<GetAccountHandler> logger)
    : IQueryHandler<GetAccountQuery, AccountDetailDto?>
{
    public async Task<AccountDetailDto?> HandleAsync(GetAccountQuery query, CancellationToken ct = default)
    {
        var dto = await context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == query.AccountId)
            .Select(a => new AccountDetailDto
            {
                Id = a.Id,
                Name = a.Name,
                IsActive = a.IsActive,
                CreatedUtc = a.CreatedUtc,
                AccountTypeId = a.AccountTypeId,
                AccountTypeName = a.AccountType.Name,
                Portfolios = a.Portfolios.Select(p => new PortfolioSummaryDto
                {
                    Id = p.Id,
                    CreatedUtc = p.CreatedUtc,
                    IsActive = p.IsActive,
                    Name = p.Name
                }).ToList()
            })
            .SingleOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "Account with id {AccountId} not found", query.AccountId);
        }
        
        return dto;
    }
}