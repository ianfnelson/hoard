using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.InstrumentTypes;

public record GetInstrumentTypeQuery(int InstrumentTypeId) : IQuery<InstrumentTypeDto?>;

public class GetInstrumentTypeHandler(HoardContext context, ILogger<GetInstrumentTypeHandler> logger)
    : IQueryHandler<GetInstrumentTypeQuery, InstrumentTypeDto?>
{
    public async Task<InstrumentTypeDto?> HandleAsync(GetInstrumentTypeQuery query, CancellationToken ct = default)
    {
        var dto = await context.InstrumentTypes
            .AsNoTracking()
            .Where(i => i.Id == query.InstrumentTypeId)
            .Select(i => new InstrumentTypeDto
            {
                Id = i.Id,
                Code = i.Code,
                Name = i.Name,
                IsCash = i.IsCash,
                IsFxPair = i.IsFxPair
            })
            .SingleOrDefaultAsync(ct);
        
        if (dto == null)
        {
            logger.LogWarning(
                "InstrumentType with id {InstrumentTypeId} not found", query.InstrumentTypeId);
        }
        
        return dto;
    }
}