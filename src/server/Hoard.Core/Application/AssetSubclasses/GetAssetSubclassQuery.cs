using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hoard.Core.Application.AssetSubclasses;

public record GetAssetSubclassQuery(int AssetSubclassId) : IQuery<AssetSubclassDto?>;

public class GetAssetSubclassHandler(HoardContext context, ILogger<GetAssetSubclassHandler> logger)
    : IQueryHandler<GetAssetSubclassQuery, AssetSubclassDto?>
{
    public async Task<AssetSubclassDto?> HandleAsync(GetAssetSubclassQuery query, CancellationToken ct = default)
    {
        var dto = await context.AssetSubclasses
            .AsNoTracking()
            .Where(a => a.Id == query.AssetSubclassId)
            .Select(a => new AssetSubclassDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                AssetClassId = a.AssetClass.Id,
                AssetClassCode = a.AssetClass.Code,
                AssetClassName = a.AssetClass.Name
            })
            .SingleOrDefaultAsync(ct);

        if (dto == null)
        {
            logger.LogWarning(
                "AssetSubclass with id {AssetSubclassId} not found", query.AssetSubclassId);
        }

        return dto;
    }
}