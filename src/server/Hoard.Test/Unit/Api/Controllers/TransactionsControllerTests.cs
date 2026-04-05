using FluentValidation;
using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class TransactionsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IValidator<TransactionWriteDto>> _validator = new();
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        _controller = new TransactionsController(_mediator.Object, _validator.Object);
    }

    [Fact]
    public async Task Get_ExistingTransaction_ReturnsOk()
    {
        var expected = new TransactionDetailDto { AccountName = "ISA", TransactionTypeName = "Buy" };
        _mediator
            .Setup(m => m.QueryAsync<GetTransactionQuery, TransactionDetailDto?>(
                It.Is<GetTransactionQuery>(q => q.TransactionId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentTransaction_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetTransactionQuery, TransactionDetailDto?>(
                It.IsAny<GetTransactionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithPagedResults()
    {
        var expected = new PagedResult<TransactionSummaryDto> { Items = [], TotalCount = 0 };
        _mediator
            .Setup(m => m.QueryAsync<GetTransactionsQuery, PagedResult<TransactionSummaryDto>>(
                It.IsAny<GetTransactionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new GetTransactionsQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var dto = new TransactionWriteDto { AccountId = 1, TransactionTypeId = 1 };
        SetupValidatorSuccess();
        _mediator
            .Setup(m => m.SendAsync<CreateTransactionCommand, int>(
                It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var result = await _controller.Create(dto, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(42, created.Value);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new TransactionWriteDto();
        SetupValidatorFailure();

        var result = await _controller.Create(dto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mediator.Verify(m => m.SendAsync<CreateTransactionCommand, int>(
            It.IsAny<CreateTransactionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsNoContent()
    {
        var dto = new TransactionWriteDto { AccountId = 1, TransactionTypeId = 1 };
        SetupValidatorSuccess();

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<UpdateTransactionCommand>(c => c.TransactionId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidRequest_ReturnsBadRequest()
    {
        var dto = new TransactionWriteDto();
        SetupValidatorFailure();

        var result = await _controller.Update(1, dto, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.IsAny<UpdateTransactionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _mediator.Verify(m => m.SendAsync(
            It.Is<DeleteTransactionCommand>(c => c.TransactionId == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadContractNote_NullFile_ReturnsBadRequest()
    {
        var result = await _controller.UploadContractNote(1, null!, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("No file uploaded", bad.Value);
    }

    [Fact]
    public async Task UploadContractNote_NonPdfFile_ReturnsBadRequest()
    {
        var file = CreateMockFile("test_ref.txt", "text/plain", 100);

        var result = await _controller.UploadContractNote(1, file, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Only PDF files are allowed", bad.Value);
    }

    [Fact]
    public async Task UploadContractNote_OversizedFile_ReturnsBadRequest()
    {
        var file = CreateMockFile("ref_note.pdf", "application/pdf", 11 * 1024 * 1024);

        var result = await _controller.UploadContractNote(1, file, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("File size exceeds 10MB limit", bad.Value);
    }

    [Fact]
    public async Task UploadContractNote_ValidPdf_ReturnsOk()
    {
        var file = CreateMockFile("REF123_note.pdf", "application/pdf", 1024);
        _mediator
            .Setup(m => m.SendAsync<UploadContractNoteCommand, string>(
                It.IsAny<UploadContractNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://blob.example.com/ref123.pdf");

        var result = await _controller.UploadContractNote(1, file, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ContractNoteUploadResponse>(ok.Value);
        Assert.Equal("REF123", response.Reference);
    }

    [Fact]
    public async Task UploadContractNote_TransactionNotFound_ReturnsNotFound()
    {
        var file = CreateMockFile("REF123_note.pdf", "application/pdf", 1024);
        _mediator
            .Setup(m => m.SendAsync<UploadContractNoteCommand, string>(
                It.IsAny<UploadContractNoteCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Transaction not found"));

        var result = await _controller.UploadContractNote(1, file, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DownloadContractNote_Exists_ReturnsFile()
    {
        var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]); // %PDF
        var contractNote = new ContractNoteResult(stream, "application/pdf", "note.pdf");
        _mediator
            .Setup(m => m.QueryAsync<GetContractNoteQuery, ContractNoteResult>(
                It.IsAny<GetContractNoteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contractNote);

        var result = await _controller.DownloadContractNote(1, CancellationToken.None);

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
    }

    [Fact]
    public async Task DownloadContractNote_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetContractNoteQuery, ContractNoteResult>(
                It.IsAny<GetContractNoteQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var result = await _controller.DownloadContractNote(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteContractNote_Success_ReturnsNoContent()
    {
        var result = await _controller.DeleteContractNote(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteContractNote_NotFound_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.SendAsync(
                It.IsAny<DeleteContractNoteCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var result = await _controller.DeleteContractNote(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private void SetupValidatorSuccess()
    {
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TransactionWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void SetupValidatorFailure()
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Field", "Error message")
        };
        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TransactionWriteDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));
    }

    private static IFormFile CreateMockFile(string fileName, string contentType, long length)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(length);
        mock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[length < 1024 ? length : 0]));
        return mock.Object;
    }
}
