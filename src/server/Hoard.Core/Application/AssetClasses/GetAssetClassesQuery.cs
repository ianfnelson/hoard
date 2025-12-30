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
                Subclasses = ac.Subclasses
                    .Select(s => new AssetSubclassDto
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Name = s.Name
                    })
                    .OrderBy(s => s.Name)
                    .ToList()
            })
            .OrderBy(ac => ac.Name)
            .ToListAsync(ct);

        return dtos;
    }
}