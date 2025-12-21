using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.AssetClasses;

public record GetAssetClassesQuery : IQuery<List<AssetClassDto>>;

public class GetAssetClassesHandler(HoardContext context)
    : IQueryHandler<GetAssetClassesQuery, List<AssetClassDto>>
{
    public async Task<List<AssetClassDto>> HandleAsync(GetAssetClassesQuery query, CancellationToken ct = default)
    {
        var dtos = await context.AssetClasses
            .AsNoTracking()
            .Select(ac => new AssetClassDto
            {
                Id = ac.Id,
                Code = ac.Code,
                Name = ac.Name,
                CreatedUtc = ac.CreatedUtc,
            }).ToListAsync(ct);

        return dtos;
    }
}