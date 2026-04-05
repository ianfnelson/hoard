using FluentValidation;
using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Instruments;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class InstrumentsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<InstrumentWriteDto>> _validator = new();
    private readonly InstrumentsController _controller;

    public InstrumentsControllerTests()
    {
        _controller = new InstrumentsController(_mediator.Object, _validator.Object);
    }

    [Fact]
    public async Task Get_ExistingInstrument_ReturnsOk()
    {
        var expected = new InstrumentDetailDto
        {
            Name = "ACME", Ticker = "ACM", InstrumentTypeName = "Share",
            AssetClassName = "Equity", AssetSubclassName = "UK",
            CurrencyId = "GBP", CurrencyName = "Pound Sterling"
        };
        _mediator
            .Setup(m => m.QueryAsync<GetInstrumentQuery, InstrumentDetailDto?>(
                It.Is<GetInstrumentQuery>(q => q.InstrumentId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentInstrument_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetInstrumentQuery, InstrumentDetailDto?>(
                It.IsAny<GetInstrumentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InstrumentDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithPagedResults()
    {
        var expected = new PagedResult<InstrumentSummaryDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetInstrumentsQuery, PagedResult<InstrumentSummaryDto>>(
                It.IsAny<GetInstrumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new GetInstrumentsQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetLookup_ReturnsOkWithLookupList()
    {
        var expected = new List<LookupDto> { new(1, "ACME Corp", "ACM") };
        _mediator
            .Setup(m => m.QueryAsync<GetInstrumentsLookupQuery, List<LookupDto>>(
                It.IsAny<GetInstrumentsLookupQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetLookup(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetPriceSeries_Exists_ReturnsOk()
    {
        var expected = new PriceSeriesDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPriceSeriesQuery, PriceSeriesDto?>(
                It.IsAny<GetPriceSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetPriceSeries(1, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetPriceSeries_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPriceSeriesQuery, PriceSeriesDto?>(
                It.IsAny<GetPriceSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceSeriesDto?)null);

        var result = await _controller.GetPriceSeries(999, null, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetValuationSeries_Exists_ReturnsOk()
    {
        var expected = new ValuationSeriesDto();
        _mediator
            .Setup(m => m.QueryAsync<GetValuationSeriesQuery, ValuationSeriesDto?>(
                It.IsAny<GetValuationSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetValuationSeries(1, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetValuationSeries_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetValuationSeriesQuery, ValuationSeriesDto?>(
                It.IsAny<GetValuationSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValuationSeriesDto?)null);

        var result = await _controller.GetValuationSeries(999, null, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPrices_Exists_ReturnsOk()
    {
        var expected = new PagedResult<PriceSummaryDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetPricesQuery, PagedResult<PriceSummaryDto>?>(
                It.IsAny<GetPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetPrices(1, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetPrices_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPricesQuery, PagedResult<PriceSummaryDto>?>(
                It.IsAny<GetPricesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResult<PriceSummaryDto>?)null);

        var result = await _controller.GetPrices(999, ct: CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var dto = new InstrumentWriteDto { Name = "ACME", InstrumentTypeId = 1 };
        SetupValidatorSuccess();
        _mediator
            .Setup(m => m.SendAsync<CreateInstrumentCommand, int>(
                It.IsAny<CreateInstrumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var result = await _controller.Create(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(42, created.Value);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new InstrumentWriteDto();
        SetupValidatorFailure();

        var result = await _controller.Create(dto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mediator.Verify(m => m.SendAsync<CreateInstrumentCommand, int>(
            It.IsAny<CreateInstrumentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsNoContent()
    {
        var dto = new InstrumentWriteDto { Name = "ACME" };
        SetupValidatorSuccess();

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<UpdateInstrumentCommand>(c => c.InstrumentId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new InstrumentWriteDto();
        SetupValidatorFailure();

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<DeleteInstrumentCommand>(c => c.InstrumentId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupValidatorSuccess()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<InstrumentWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void SetupValidatorFailure()
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Name", "Name is required")
        };
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<InstrumentWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));
    }
}
