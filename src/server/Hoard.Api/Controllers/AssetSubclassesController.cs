using Hoard.Core.Application;
using Hoard.Core.Application.AssetSubclasses.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/[controller]")]
[Tags("Reference")]
[Produces("application/json")]
public class AssetSubclassesController(IMediator mediator, ILogger<AssetSubclassesController> logger)
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