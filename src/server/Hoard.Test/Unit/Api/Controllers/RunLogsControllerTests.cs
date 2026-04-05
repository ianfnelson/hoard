using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.RunLog;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class RunLogsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RunLogsController _controller;

    public RunLogsControllerTests()
    {
        _controller = new RunLogsController(_mediator.Object);
    }

    [Fact]
    public async Task GetRunLogs_ReturnsOkWithPagedResults()
    {
        var expected = new PagedResult<RunLogDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetRunLogsQuery, PagedResult<RunLogDto>>(
                It.IsAny<GetRunLogsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetRunLogsAsync(new GetRunLogsQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }
}
