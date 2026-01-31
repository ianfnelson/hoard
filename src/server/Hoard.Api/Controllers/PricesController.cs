using Hoard.Core.Application;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/prices/")]
[Produces("application/json")]
[Tags("Prices")]
public class PricesController(IMediator mediator)
{
    // TODO - endpoint for upserting prices
    // TODO - endpoint for deleting prices
    // TODO - endpoint for bucketed price series
}