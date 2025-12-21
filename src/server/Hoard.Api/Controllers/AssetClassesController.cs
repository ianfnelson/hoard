using Hoard.Core.Application;
using Hoard.Core.Application.AssetClasses;
using Hoard.Core.Application.AssetSubclasses;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/asset-classes/")]
[Tags("Reference")]
[Produces("application/json")]
public class AssetClassesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AssetClassDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetClassDto>>> GetList(CancellationToken ct)
    {
        var query = new GetAssetClassesQuery();
        
        var dtos = await mediator.QueryAsync<GetAssetClassesQuery, List<AssetClassDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AssetClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetClassDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetAssetClassQuery(id);
        
        var dto = await mediator.QueryAsync<GetAssetClassQuery, AssetClassDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/subcategories")]
    public async Task<ActionResult<List<AssetSubclassDto>>> GetSubclasses(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}