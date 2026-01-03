using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/portfolios/")]
[Tags("Portfolios")]
[Produces("application/json")]
public class PortfoliosController(IMediator mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<PortfolioSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PortfolioSummaryDto>>> GetList(CancellationToken ct)
    {
        var query = new GetPortfoliosQuery();
        
        var dtos = await mediator.QueryAsync<GetPortfoliosQuery, List<PortfolioSummaryDto>>(query, ct);
        
        return new OkObjectResult(dtos);
    }
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetPortfolioQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioQuery, PortfolioDetailDto?>(query, ct);

        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/valuations/latest")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioValuationDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioValuationDetailDto>> GetLatestValuation(int id, CancellationToken ct)
    {
        var query = new GetPortfolioValuationQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioValuationQuery, PortfolioValuationDetailDto?>(query, ct);

        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }

    [HttpGet("{id:int}/instrument-types")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioInstrumentTypesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioInstrumentTypesDto>> GetInstrumentTypes(int id, CancellationToken ct)
    {
        var query = new GetPortfolioInstrumentTypesQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioInstrumentTypesQuery, PortfolioInstrumentTypesDto?>(query, ct);
        
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/exposure")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioExposureDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioExposureDto>> GetExposure(int id, CancellationToken ct)
    {
        var query = new GetPortfolioExposureQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioExposureQuery, PortfolioExposureDto?>(query, ct);
        
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/snapshots")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioSnapshotsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioSnapshotsDto?>> GetSnapshots(int id, CancellationToken ct)
    {
        var query = new GetPortfolioSnapshotsQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioSnapshotsQuery, PortfolioSnapshotsDto?>(query, ct);

        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet("{id:int}/positions")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PortfolioPositionsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PortfolioPositionsDto>> GetPositions(int id, [FromQuery] PositionStatus? status, CancellationToken ct)
    {
        // This needs more filters, sort, search in the query object. 
        
        var query = new GetPortfolioPositionsQuery(id);
        
        var dto = await mediator.QueryAsync<GetPortfolioPositionsQuery, PortfolioPositionsDto?>(query, ct);
        
        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    // TODO - add target allocations put endpoint
    // TODO - valuations, for charts and export
}