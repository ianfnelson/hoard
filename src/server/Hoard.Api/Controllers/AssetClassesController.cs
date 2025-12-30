using Hoard.Core.Application;
using Hoard.Core.Application.AssetClasses;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/reference/asset-classes/")]
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
}