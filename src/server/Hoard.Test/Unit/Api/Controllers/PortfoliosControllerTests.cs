using FluentValidation;
using FluentValidation.Results;
using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Portfolios;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class PortfoliosControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<SetTargetAllocationsRequest>> _targetAllocationsValidator = new();
    private readonly PortfoliosController _controller;

    public PortfoliosControllerTests()
    {
        _targetAllocationsValidator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _controller = new PortfoliosController(_mediator.Object, _targetAllocationsValidator.Object);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithPortfolios()
    {
        var expected = new List<PortfolioSummaryDto>();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfoliosQuery, List<PortfolioSummaryDto>>(
                It.IsAny<GetPortfoliosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_ExistingPortfolio_ReturnsOk()
    {
        var expected = new PortfolioDetailDto { Name = "Main" };
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioQuery, PortfolioDetailDto?>(
                It.Is<GetPortfolioQuery>(q => q.PortfolioId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentPortfolio_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioQuery, PortfolioDetailDto?>(
                It.IsAny<GetPortfolioQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetInstrumentTypes_Exists_ReturnsOk()
    {
        var expected = new PortfolioInstrumentTypesDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioInstrumentTypesQuery, PortfolioInstrumentTypesDto?>(
                It.IsAny<GetPortfolioInstrumentTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetInstrumentTypes(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetInstrumentTypes_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioInstrumentTypesQuery, PortfolioInstrumentTypesDto?>(
                It.IsAny<GetPortfolioInstrumentTypesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioInstrumentTypesDto?)null);

        var result = await _controller.GetInstrumentTypes(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetExposure_Exists_ReturnsOk()
    {
        var expected = new PortfolioExposureDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioExposureQuery, PortfolioExposureDto?>(
                It.IsAny<GetPortfolioExposureQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetExposure(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetExposure_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioExposureQuery, PortfolioExposureDto?>(
                It.IsAny<GetPortfolioExposureQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioExposureDto?)null);

        var result = await _controller.GetExposure(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetSnapshots_Exists_ReturnsOk()
    {
        var expected = new PortfolioSnapshotsDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioSnapshotsQuery, PortfolioSnapshotsDto?>(
                It.IsAny<GetPortfolioSnapshotsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetSnapshots(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetSnapshots_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioSnapshotsQuery, PortfolioSnapshotsDto?>(
                It.IsAny<GetPortfolioSnapshotsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioSnapshotsDto?)null);

        var result = await _controller.GetSnapshots(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPositions_Exists_ReturnsOk()
    {
        var expected = new PortfolioPositionsDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioPositionsQuery, PortfolioPositionsDto?>(
                It.IsAny<GetPortfolioPositionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetPositions(1, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetPositions_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioPositionsQuery, PortfolioPositionsDto?>(
                It.IsAny<GetPortfolioPositionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioPositionsDto?)null);

        var result = await _controller.GetPositions(999, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetValuation_Exists_ReturnsOk()
    {
        var expected = new PortfolioValuationsDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioValuationsQuery, PortfolioValuationsDto?>(
                It.IsAny<GetPortfolioValuationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetValuation(1, null, null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetValuation_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioValuationsQuery, PortfolioValuationsDto?>(
                It.IsAny<GetPortfolioValuationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioValuationsDto?)null);

        var result = await _controller.GetValuation(999, null, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetValuation_ArgumentException_ReturnsBadRequest()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioValuationsQuery, PortfolioValuationsDto?>(
                It.IsAny<GetPortfolioValuationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("From date must be before To date"));

        var result = await _controller.GetValuation(1, new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetValuationSeries_Exists_ReturnsOk()
    {
        var expected = new PortfolioValuationSeriesDto();
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioValuationSeriesQuery, PortfolioValuationSeriesDto?>(
                It.IsAny<GetPortfolioValuationSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetValuationSeries(1, null, null, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetValuationSeries_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPortfolioValuationSeriesQuery, PortfolioValuationSeriesDto?>(
                It.IsAny<GetPortfolioValuationSeriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioValuationSeriesDto?)null);

        var result = await _controller.GetValuationSeries(999, null, null, ct: CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ExportValuationPdf_Exists_ReturnsFile()
    {
        var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]);
        var pdfResult = new PortfolioValuationsPdfResult(stream, "application/pdf", "valuation.pdf");
        _mediator
            .Setup(m => m.QueryAsync<ExportPortfolioValuationsPdfQuery, PortfolioValuationsPdfResult?>(
                It.IsAny<ExportPortfolioValuationsPdfQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfResult);

        var result = await _controller.ExportValuationPdf(1, null, null, CancellationToken.None);

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("valuation.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExportValuationPdf_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<ExportPortfolioValuationsPdfQuery, PortfolioValuationsPdfResult?>(
                It.IsAny<ExportPortfolioValuationsPdfQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioValuationsPdfResult?)null);

        var result = await _controller.ExportValuationPdf(1, null, null, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ExportValuationPdf_ArgumentException_ReturnsBadRequest()
    {
        _mediator
            .Setup(m => m.QueryAsync<ExportPortfolioValuationsPdfQuery, PortfolioValuationsPdfResult?>(
                It.IsAny<ExportPortfolioValuationsPdfQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid date range"));

        var result = await _controller.ExportValuationPdf(1, new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetTargetAllocations_Valid_ReturnsNoContent()
    {
        var request = new SetTargetAllocationsRequest();

        var result = await _controller.SetTargetAllocations(1, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<SetTargetAllocationsCommand>(c => c.PortfolioId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetTargetAllocations_InvalidValidation_ReturnsBadRequest()
    {
        _targetAllocationsValidator
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Allocations", "Target allocations must not exceed 100%")]));

        var result = await _controller.SetTargetAllocations(1, new SetTargetAllocationsRequest(), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _mediator.Verify(m => m.SendAsync(It.IsAny<SetTargetAllocationsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetTargetAllocations_PortfolioNotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.SendAsync(It.IsAny<SetTargetAllocationsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Portfolio 999 not found."));

        var result = await _controller.SetTargetAllocations(999, new SetTargetAllocationsRequest(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }
}
