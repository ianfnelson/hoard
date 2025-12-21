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
    public async Task<ActionResult<List<AssetClassDto>>> GetList(CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetClassDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}/subcategories")]
    public async Task<ActionResult<List<AssetSubclassDto>>> GetSubclasses(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}