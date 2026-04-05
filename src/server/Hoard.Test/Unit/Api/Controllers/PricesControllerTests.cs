using FluentValidation;
using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Prices;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class PricesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<PriceWriteDto>> _validator = new();
    private readonly PricesController _controller;

    public PricesControllerTests()
    {
        _controller = new PricesController(_mediator.Object, _validator.Object);
    }

    [Fact]
    public async Task Get_ExistingPrice_ReturnsOk()
    {
        var expected = new PriceDetailDto
        {
            Id = 1, InstrumentId = 1, InstrumentName = "ACME", InstrumentTicker = "ACM",
            AsOfDate = new DateOnly(2024, 1, 15), RetrievedUtc = DateTime.UtcNow,
            Source = "Yahoo", IsLocked = false, Close = 100m, AdjustedClose = 100m
        };
        _mediator
            .Setup(m => m.QueryAsync<GetPriceQuery, PriceDetailDto?>(
                It.Is<GetPriceQuery>(q => q.PriceId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentPrice_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetPriceQuery, PriceDetailDto?>(
                It.IsAny<GetPriceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var dto = new PriceWriteDto { InstrumentId = 1, Close = 100m };
        SetupValidatorSuccess();
        _mediator
            .Setup(m => m.SendAsync<CreatePriceCommand, int>(
                It.IsAny<CreatePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var result = await _controller.Create(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(42, created.Value);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new PriceWriteDto();
        SetupValidatorFailure();

        var result = await _controller.Create(dto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mediator.Verify(m => m.SendAsync<CreatePriceCommand, int>(
            It.IsAny<CreatePriceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsNoContent()
    {
        var dto = new PriceWriteDto { InstrumentId = 1, Close = 100m };
        SetupValidatorSuccess();

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<UpdatePriceCommand>(c => c.PriceId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new PriceWriteDto();
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
            It.Is<DeletePriceCommand>(c => c.PriceId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupValidatorSuccess()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<PriceWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void SetupValidatorFailure()
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Close", "Close is required")
        };
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<PriceWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));
    }
}
