using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Core.Infrastructure.Integrations;
using Hoard.Messages;
using Hoard.Messages.Transactions;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class DeleteTransactionHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus = new();
    private readonly Mock<IBlobStorageService> _blobService = new();
    private readonly DeleteTransactionHandler _handler;

    public DeleteTransactionHandlerTests()
    {
        _context = TestContextFactory.Create();
        _handler = new DeleteTransactionHandler(_context, _bus.Object, _blobService.Object);
    }

    [Fact]
    public async Task HandleAsync_DeletesTransactionAndPublishesEvent()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        var transaction = TestData.BuyTransaction(accountId: account.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var transactionId = transaction.Id;
        var transactionDate = transaction.Date;

        await _handler.HandleAsync(new DeleteTransactionCommand(transactionId));

        var exists = await _context.Transactions.AnyAsync(t => t.Id == transactionId);
        Assert.False(exists);

        _bus.Verify(b => b.Publish(It.Is<TransactionDeletedEvent>(e =>
            e.Id == transactionId && e.Date == transactionDate), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithContractNote_DeletesBlobBeforeTransaction()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        var transaction = TestData.BuyTransaction(accountId: account.Id);
        transaction.ContractNoteReference = "contracts/note-123.pdf";
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new DeleteTransactionCommand(transaction.Id));

        _blobService.Verify(b => b.DeleteContractNoteAsync("contracts/note-123.pdf", default), Times.Once);

        var exists = await _context.Transactions.AnyAsync(t => t.Id == transaction.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task HandleAsync_WithoutContractNote_DoesNotCallBlobService()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        var transaction = TestData.BuyTransaction(accountId: account.Id);
        transaction.ContractNoteReference = null;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new DeleteTransactionCommand(transaction.Id));

        _blobService.Verify(b => b.DeleteContractNoteAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_TransactionNotFound_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.HandleAsync(new DeleteTransactionCommand(999)));
    }

    [Fact]
    public async Task HandleAsync_UsesPipelineModeFromCommand()
    {
        var account = TestData.Account();
        _context.Accounts.Add(account);

        var transaction = TestData.BuyTransaction(accountId: account.Id);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _handler.HandleAsync(new DeleteTransactionCommand(transaction.Id, PipelineMode.Backfill));

        _bus.Verify(b => b.Publish(It.Is<TransactionDeletedEvent>(e =>
            e.PipelineMode == PipelineMode.Backfill), null), Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
