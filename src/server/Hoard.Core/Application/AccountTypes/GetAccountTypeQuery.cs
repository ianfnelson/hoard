using Hoard.Core.Application.AccountTypes.Models;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.AccountTypes;

public record GetAccountTypeQuery(int AccountTypeId) : IQuery<AccountTypeDto?>;

public class GetAccountTypeHandler(HoardContext context, ILogger<GetAccountTypeHandler> logger)
    : IQueryHandler<GetAccountTypeQuery, AccountTypeDto?>
{
    public async Task<AccountTypeDto?> HandleAsync(GetAccountTypeQuery query, CancellationToken ct = default)
    {
        var dto = await context.AccountTypes
            .AsNoTracking()
            .Where(at => at.Id == query.AccountTypeId)
            .Select(at => new AccountTypeDto
            {
                Id = at.Id,
                Name = at.Name,
                CreatedUtc = at.CreatedUtc
            })
            .SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "AccountType with id {AccountTypeId} not found", query.AccountTypeId);
        }

        return dto;
    }
}