using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.RunLog;

public sealed record RunTypeDto(int Id, string Name);

public sealed class GetRunTypesQuery : IQuery<IReadOnlyList<RunTypeDto>>;

public sealed class GetRunTypesHandler(HoardContext context)
    : IQueryHandler<GetRunTypesQuery, IReadOnlyList<RunTypeDto>>
{
    public async Task<IReadOnlyList<RunTypeDto>> HandleAsync(GetRunTypesQuery query, CancellationToken ct = default)
    {
        return await context.RunTypes
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RunTypeDto(r.Id, r.Name))
            .ToListAsync(ct);
    }
}
