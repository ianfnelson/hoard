using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.AlertTypes;

public record GetAlertTypesQuery : IQuery<List<AlertTypeDto>>;

public class GetAlertTypesHandler(HoardContext context)
    : IQueryHandler<GetAlertTypesQuery, List<AlertTypeDto>>
{
    public Task<List<AlertTypeDto>> HandleAsync(GetAlertTypesQuery query, CancellationToken ct = default)
    {
        var dtos = context.AlertTypes
            .AsNoTracking()
            .Select(at => new AlertTypeDto
            {
                Id = at.Id,
                Name = at.Name
            })
            .OrderBy(at => at.Name)
            .ToListAsync(ct);

        return dtos;
    }
}
