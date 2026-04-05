using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Alerts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class AlertsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AlertsController _controller;

    public AlertsControllerTests()
    {
        _controller = new AlertsController(_mediator.Object);
    }

    [Fact]
    public async Task Get_ExistingAlert_ReturnsOk()
    {
        var expected = new AlertDetailDto
        {
            Id = 1, AlertTypeName = "Price", Title = "Alert", Message = "Test"
        };
        _mediator
            .Setup(m => m.QueryAsync<GetAlertQuery, AlertDetailDto?>(
                It.Is<GetAlertQuery>(q => q.AlertId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentAlert_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetAlertQuery, AlertDetailDto?>(
                It.IsAny<GetAlertQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithPagedAlerts()
    {
        var expected = new PagedResult<AlertSummaryDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetAlertsQuery, PagedResult<AlertSummaryDto>>(
                It.IsAny<GetAlertsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new GetAlertsQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}
