using Hoard.Core.Application;
using Hoard.Core.Application.Currencies.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/[controller]")]
[Tags("Reference")]
[Produces("application/json")]
public class CurrenciesController(IMediator mediator, ILogger<CurrenciesController> logger)
{
    [HttpGet]
    public async Task<ActionResult<List<CurrencyDto>>> GetList(CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CurrencyDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}