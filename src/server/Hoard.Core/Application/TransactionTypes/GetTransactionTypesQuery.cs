using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.TransactionTypes;

public record GetTransactionTypesQuery : IQuery<List<TransactionTypeDto>>;

public class GetTransactionTypesHandler(HoardContext context)
    : IQueryHandler<GetTransactionTypesQuery, List<TransactionTypeDto>>
{
    public Task<List<TransactionTypeDto>> HandleAsync(GetTransactionTypesQuery query, CancellationToken ct = default)
    {
        var dtos = context.TransactionTypes
            .AsNoTracking()
            .Select(i => new TransactionTypeDto
            {
                Id = i.Id,
                Name = i.Name
            })
            .OrderBy(i => i.Name)
            .ToListAsync(ct);

        return dtos;
    }
}