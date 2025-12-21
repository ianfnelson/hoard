using Hoard.Api.Models;
using Hoard.Core.Application;
using Hoard.Core.Application.Accounts;
using Hoard.Core.Application.Portfolios;
using Hoard.Core.Application.Positions;
using Hoard.Core.Application.Snapshots;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("portfolios/")]
[Tags("Portfolios")]
[Produces("application/json")]
public class PortfoliosController(IMediator mediator)
{
    [HttpGet]
    public async Task<ActionResult<List<PortfolioSummaryDto>>> GetList(CancellationToken ct)
    {
        var query = new GetPortfoliosQuery();
        
        var dtos = await mediator.QueryAsync<GetPortfoliosQuery, List<PortfolioSummaryDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PortfolioSummaryDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetPortfolioQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioQuery, PortfolioDetailDto?>(query, ct);

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
    
    [HttpGet("{id:int}/valuations/latest")]
    public async Task<ActionResult<PortfolioValuationDetailDto>> GetLatestValuation(int id, CancellationToken ct)
    {
        var query = new GetPortfolioValuationQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioValuationQuery, PortfolioValuationDetailDto?>(query, ct);

        if (dto == null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/valuations/series")]
    public async Task<ActionResult<List<PortfolioValuationSummaryDto>>> GetValuations(int id, [FromQuery]SeriesOptions seriesOptions, CancellationToken ct)
    {
        // TODO - buckets
        var query = new GetPortfolioValuationsQuery(id, seriesOptions.From, seriesOptions.To);
        
        var dtos = await mediator.QueryAsync<GetPortfolioValuationsQuery, List<PortfolioValuationSummaryDto>>(query, ct);

        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}/valuations")]
    public async Task<ActionResult<PagedResult<PortfolioValuationSummaryDto>>> GetValuations(int id, [FromQuery]PagedOptions pagedOptions, CancellationToken ct)
    {
        // TODO - is PagedOptions sufficient here, maybe it needs sort properties too?
        // TODO 
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}/instrument-types")]
    public async Task<ActionResult<List<PortfolioInstrumentTypeDto>>> GetInstrumentTypes(int id, CancellationToken ct)
    {
        // TODO - change this to return an envelope, not a bare list
        var query = new GetPortfolioInstrumentTypesQuery(id);
        
        var dtos = await mediator.QueryAsync<GetPortfolioInstrumentTypesQuery, List<PortfolioInstrumentTypeDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}/exposure")]
    public async Task<ActionResult<PortfolioExposureDto>> GetExposure(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}/accounts")]
    public async Task<ActionResult<List<AccountSummaryDto>>> GetAccounts(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}/snapshots")]
    public async Task<ActionResult<List<SnapshotDto>>> GetSnapshots(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}/positions")]
    public async Task<ActionResult<List<PositionSummaryDto>>> GetPositions(int id, [FromQuery] PositionStatus? status, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:int}/snapshots/latest")]
    public async Task<ActionResult<SnapshotDto>> GetLatestSnapshot(int id, CancellationToken ct)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    // TODO - add target allocations put endpoint
}