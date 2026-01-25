using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Microsoft.AspNetCore.Mvc;

namespace Hoard.Api.Controllers;

[ApiController]
[Route("api/news-articles/")]
[Produces("application/json")]
[Tags("News")]
public class NewsArticlesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(NewsArticleDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NewsArticleDetailDto>> Get(int id, CancellationToken ct)
    {
        var query = new GetNewsArticleQuery(id);
        
        var dto = await mediator.QueryAsync<GetNewsArticleQuery, NewsArticleDetailDto?>(query, ct);

        return dto == null ? new NotFoundResult() : new OkObjectResult(dto);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NewsArticleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NewsArticleSummaryDto>>> GetList([FromQuery] GetNewsArticlesQuery query, CancellationToken ct)
    {
        var dtos = await mediator.QueryAsync<GetNewsArticlesQuery, PagedResult<NewsArticleSummaryDto>>(query, ct);
        return new OkObjectResult(dtos);
    }
}