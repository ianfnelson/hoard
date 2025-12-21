using Hoard.Core.Application;
using Hoard.Core.Application.AccountTypes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("reference/[controller]")]
[Tags("Reference")]
[Produces("application/json")]
public class AccountTypesController(IMediator mediator, ILogger<AccountTypesController> logger)
{
    [HttpGet]
    public async Task<ActionResult<List<AccountTypeDto>>> GetList(CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountTypeDto>> Get(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
}