using Hoard.Core.Application.AccountTypes;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.AssetClasses;

public record GetAssetClassQuery(int AssetClassId) : IQuery<AssetClassDto?>;

public class GetAssetClassHandler(HoardContext context, ILogger<GetAssetClassHandler> logger)
    : IQueryHandler<GetAssetClassQuery, AssetClassDto?>
{
    public async Task<AssetClassDto?> HandleAsync(GetAssetClassQuery query, CancellationToken ct = default)
    {
        var dto = await context.AssetClasses
            .AsNoTracking()
            .Where(ac => ac.Id == query.AssetClassId)
            .Select(ac => new AssetClassDto
            {
                Id = ac.Id,
                Code = ac.Code,
                Name = ac.Name,
                CreatedUtc = ac.CreatedUtc
            })
            .SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "AssetClass with id {AssetClassId} not found", query.AssetClassId);
        }

        return dto;
    }
}