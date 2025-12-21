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
    public async Task<ActionResult<List<AssetSubclassDto>>> GetList([FromQuery] int assetClassId, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetSubclassDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}