using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.AssetSubclasses;

public record GetAssetSubclassesQuery : IQuery<List<AssetSubclassDto>>
{
    public int? AssetClassId { get; set; }
}

public class GetAssetSubclassesHandler(HoardContext context)
    : IQueryHandler<GetAssetSubclassesQuery, List<AssetSubclassDto>>
{
    public async Task<List<AssetSubclassDto>> HandleAsync(GetAssetSubclassesQuery query, CancellationToken ct = default)
    {
        var q = context.AssetSubclasses.AsNoTracking();

        if (query.AssetClassId.HasValue)
        {
            q = q.Where(a => a.AssetClassId == query.AssetClassId);
        }

        var dtos = await q
            .Select(a => new AssetSubclassDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                AssetClassId = a.AssetClass.Id,
                AssetClassCode = a.AssetClass.Code,
                AssetClassName = a.AssetClass.Name
            })
            .OrderBy(a => a.Name)
            .ToListAsync(ct);

        return dtos;
    }
}