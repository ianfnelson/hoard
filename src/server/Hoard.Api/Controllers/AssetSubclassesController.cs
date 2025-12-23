using Hoard.Core.Application;
using Hoard.Core.Application.AssetSubclasses;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/asset-subclasses/")]
[Tags("Reference")]
[Produces("application/json")]
public class AssetSubclassesController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AssetSubclassDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetSubclassDto>>> GetList([FromQuery] int? assetClassId, CancellationToken ct)
    {
        var query = new GetAssetSubclassesQuery
        {
            AssetClassId = assetClassId
        };
        
        var dtos = await mediator.QueryAsync<GetAssetSubclassesQuery, List<AssetSubclassDto>>(query, ct);
       
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AssetSubclassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetSubclassDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetAssetSubclassQuery(id);

        var dto = await mediator.QueryAsync<GetAssetSubclassQuery, AssetSubclassDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }
        
        return new OkObjectResult(dto);
    }
}