using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.News;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class NewsArticlesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly NewsArticlesController _controller;

    public NewsArticlesControllerTests()
    {
        _controller = new NewsArticlesController(_mediator.Object);
    }

    [Fact]
    public async Task Get_ExistingArticle_ReturnsOk()
    {
        var expected = new NewsArticleDetailDto
        {
            Id = 1, Headline = "Test", BodyHtml = "<p>Hi</p>",
            Url = "https://example.com", InstrumentName = "ACME", InstrumentTicker = "ACM"
        };
        _mediator
            .Setup(m => m.QueryAsync<GetNewsArticleQuery, NewsArticleDetailDto?>(
                It.Is<GetNewsArticleQuery>(q => q.NewsArticleId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentArticle_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetNewsArticleQuery, NewsArticleDetailDto?>(
                It.IsAny<GetNewsArticleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsArticleDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithPagedArticles()
    {
        var expected = new PagedResult<NewsArticleSummaryDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetNewsArticlesQuery, PagedResult<NewsArticleSummaryDto>>(
                It.IsAny<GetNewsArticlesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new GetNewsArticlesQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}
