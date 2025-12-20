using Hoard.Core.Application;
using Hoard.Core.Application.InstrumentTypes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/[controller]")]
[Tags("Reference")]
[Produces("application/json")]
public class InstrumentTypesController(IMediator mediator, ILogger<InstrumentTypesController> logger)
{
    [HttpGet]
    public async Task<ActionResult<List<InstrumentTypeDto>>> GetList(CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstrumentTypeDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}