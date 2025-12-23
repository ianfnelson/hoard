using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.InstrumentTypes;

public record GetInstrumentTypesQuery : IQuery<List<InstrumentTypeDto>>;

public class GetInstrumentTypesHandler(HoardContext context)
    : IQueryHandler<GetInstrumentTypesQuery, List<InstrumentTypeDto>>
{
    public Task<List<InstrumentTypeDto>> HandleAsync(GetInstrumentTypesQuery query, CancellationToken ct = default)
    {
        var dtos = context.InstrumentTypes
            .AsNoTracking()
            .Select(i => new InstrumentTypeDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                IsCash = i.IsCash,
                IsFxPair = i.IsFxPair
            })
            .OrderBy(i => i.Name)
            .ToListAsync(ct);

        return dtos;
    }
}
