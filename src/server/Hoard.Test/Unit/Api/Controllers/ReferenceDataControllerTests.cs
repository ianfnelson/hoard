using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.AlertTypes;
using Hoard.Core.Application.AssetClasses;
using Hoard.Core.Application.Currencies;
using Hoard.Core.Application.InstrumentTypes;
using Hoard.Core.Application.RunLog;
using Hoard.Core.Application.TransactionTypes;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class AlertTypesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithAlertTypes()
    {
        var expected = new List<AlertTypeDto> { new() { Id = 1, Name = "Price" } };
        _mediator
            .Setup(m => m.QueryAsync<GetAlertTypesQuery, List<AlertTypeDto>>(
                It.IsAny<GetAlertTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new AlertTypesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}

public class AssetClassesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithAssetClasses()
    {
        var expected = new List<AssetClassDto>
        {
            new() { Id = 1, Name = "Equity", Colour = "#0000FF", Subclasses = [] }
        };
        _mediator
            .Setup(m => m.QueryAsync<GetAssetClassesQuery, List<AssetClassDto>>(
                It.IsAny<GetAssetClassesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new AssetClassesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}

public class CurrenciesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithCurrencies()
    {
        var expected = new List<CurrencyDto> { new() { Id = "GBP", Name = "Pound Sterling" } };
        _mediator
            .Setup(m => m.QueryAsync<GetCurrenciesQuery, List<CurrencyDto>>(
                It.IsAny<GetCurrenciesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new CurrenciesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}

public class InstrumentTypesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithInstrumentTypes()
    {
        var expected = new List<InstrumentTypeDto>
        {
            new() { Id = 1, Name = "Share", Colour = "#FF0000" }
        };
        _mediator
            .Setup(m => m.QueryAsync<GetInstrumentTypesQuery, List<InstrumentTypeDto>>(
                It.IsAny<GetInstrumentTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new InstrumentTypesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}

public class TransactionTypesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithTransactionTypes()
    {
        var expected = new List<TransactionTypeDto> { new() { Id = 1, Name = "Buy" } };
        _mediator
            .Setup(m => m.QueryAsync<GetTransactionTypesQuery, List<TransactionTypeDto>>(
                It.IsAny<GetTransactionTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new TransactionTypesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}

public class RunLogTypesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task GetList_ReturnsOkWithRunTypes()
    {
        IReadOnlyList<RunTypeDto> expected = new List<RunTypeDto> { new(1, "CloseOfDay") };
        _mediator
            .Setup(m => m.QueryAsync<GetRunTypesQuery, IReadOnlyList<RunTypeDto>>(
                It.IsAny<GetRunTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new RunLogTypesController(_mediator.Object);

        var result = await controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }
}
