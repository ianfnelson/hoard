using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class UploadContractNoteHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBlobStorageService> _blobService = new();

    [Fact]
    public async Task HandleAsync_UploadsFileAndUpdatesReference()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m
        });
        await _context.SaveChangesAsync();

        _blobService.Setup(b => b.UploadContractNoteAsync("CN-001", It.IsAny<Stream>(), default))
            .ReturnsAsync("https://blob.storage/CN-001.pdf");

        var handler = new UploadContractNoteHandler(_context, _blobService.Object);
        using var stream = new MemoryStream();

        // Act
        var result = await handler.HandleAsync(new UploadContractNoteCommand(1, "CN-001", stream));

        // Assert
        Assert.Equal("https://blob.storage/CN-001.pdf", result);
        var transaction = await _context.Transactions.FindAsync(1);
        Assert.Equal("CN-001", transaction!.ContractNoteReference);
    }

    [Fact]
    public async Task HandleAsync_DeletesOldBlobWhenReferenceChanges()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m,
            ContractNoteReference = "OLD-REF"
        });
        await _context.SaveChangesAsync();

        _blobService.Setup(b => b.UploadContractNoteAsync("NEW-REF", It.IsAny<Stream>(), default))
            .ReturnsAsync("https://blob.storage/NEW-REF.pdf");

        var handler = new UploadContractNoteHandler(_context, _blobService.Object);
        using var stream = new MemoryStream();

        // Act
        await handler.HandleAsync(new UploadContractNoteCommand(1, "NEW-REF", stream));

        // Assert
        _blobService.Verify(b => b.DeleteContractNoteAsync("OLD-REF", default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TransactionNotFound_Throws()
    {
        var handler = new UploadContractNoteHandler(_context, _blobService.Object);
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(new UploadContractNoteCommand(999, "CN-001", stream)));
    }

    public void Dispose() => _context.Dispose();
}

public class DeleteContractNoteHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBlobStorageService> _blobService = new();

    [Fact]
    public async Task HandleAsync_DeletesBlobAndClearsReference()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m,
            ContractNoteReference = "CN-001"
        });
        await _context.SaveChangesAsync();

        var handler = new DeleteContractNoteHandler(_context, _blobService.Object);

        // Act
        await handler.HandleAsync(new DeleteContractNoteCommand(1));

        // Assert
        _blobService.Verify(b => b.DeleteContractNoteAsync("CN-001", default), Times.Once);
        var transaction = await _context.Transactions.FindAsync(1);
        Assert.Null(transaction!.ContractNoteReference);
    }

    [Fact]
    public async Task HandleAsync_NoContractNoteReference_DoesNothing()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m
        });
        await _context.SaveChangesAsync();

        var handler = new DeleteContractNoteHandler(_context, _blobService.Object);

        // Act
        await handler.HandleAsync(new DeleteContractNoteCommand(1));

        // Assert
        _blobService.Verify(b => b.DeleteContractNoteAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_TransactionNotFound_Throws()
    {
        var handler = new DeleteContractNoteHandler(_context, _blobService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(new DeleteContractNoteCommand(999)));
    }

    public void Dispose() => _context.Dispose();
}

public class GetContractNoteHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IBlobStorageService> _blobService = new();

    [Fact]
    public async Task HandleAsync_ReturnsContractNoteStream()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m,
            ContractNoteReference = "CN-001"
        });
        await _context.SaveChangesAsync();

        var memoryStream = new MemoryStream();
        _blobService.Setup(b => b.DownloadContractNoteAsync("CN-001", default))
            .ReturnsAsync((memoryStream, "application/pdf"));

        var handler = new GetContractNoteHandler(_context, _blobService.Object);

        // Act
        var result = await handler.HandleAsync(new GetContractNoteQuery(1));

        // Assert
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("CN-001.pdf", result.FileName);
        Assert.Same(memoryStream, result.Stream);
    }

    [Fact]
    public async Task HandleAsync_NoContractNoteReference_Throws()
    {
        // Arrange
        _context.Accounts.Add(TestData.Account());
        _context.TransactionTypes.Add(TestData.TransactionType(Hoard.Core.Domain.Entities.TransactionType.Buy));
        _context.Transactions.Add(new Transaction
        {
            Id = 1, AccountId = 1, TransactionTypeId = TransactionType.Buy,
            Date = new DateOnly(2024, 1, 15), Value = -1000m
        });
        await _context.SaveChangesAsync();

        var handler = new GetContractNoteHandler(_context, _blobService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(new GetContractNoteQuery(1)));
    }

    [Fact]
    public async Task HandleAsync_TransactionNotFound_Throws()
    {
        var handler = new GetContractNoteHandler(_context, _blobService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(new GetContractNoteQuery(999)));
    }

    public void Dispose() => _context.Dispose();
}
