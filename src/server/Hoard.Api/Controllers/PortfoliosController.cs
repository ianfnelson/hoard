using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.Portfolios.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PortfoliosController(IMediator mediator, ILogger<PortfoliosController> logger)
{
    [HttpGet]
    public async Task<ActionResult<List<PortfolioSummaryDto>>> Get(CancellationToken ct)
    {
        var query = new GetPortfoliosQuery();
        
        var dtos = await mediator.QueryAsync<GetPortfoliosQuery, List<PortfolioSummaryDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PortfolioSummaryDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetPortfolioQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioQuery, PortfolioDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/performance")]
    public async Task<ActionResult<PortfolioPerformanceDto>> GetPerformance(int id, CancellationToken ct)
    {
        var query = new GetPortfolioPerformanceQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioPerformanceQuery, PortfolioPerformanceDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/valuation/latest")]
    public async Task<ActionResult<PortfolioValuationDto>> GetLatestValuation(int id, CancellationToken ct)
    {
        var query = new GetPortfolioValuationQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioValuationQuery, PortfolioValuationDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/valuations")]
    public async Task<ActionResult<List<PortfolioValuationSummaryDto>>> GetValuations(int id, [FromQuery]DateOnly? from, [FromQuery]DateOnly? to, CancellationToken ct)
    {
        var query = new GetPortfolioValuationsQuery(id, from, to);
        
        var dto = await mediator.QueryAsync<GetPortfolioValuationsQuery, List<PortfolioValuationSummaryDto>>(query, ct);

        return new OkObjectResult(dto);
    }
}