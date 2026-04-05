using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Prices;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class CreateTransactionHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus;
    private readonly Mock<IMapper> _mapper;
    private readonly CreateTransactionHandler _handler;

    public CreateTransactionHandlerTests()
    {
        _context = TestContextFactory.Create();

        _bus = new Mock<IBus>();
        _mapper = new Mock<IMapper>();
        _handler = new CreateTransactionHandler(_bus.Object, _mapper.Object, _context);
    }

    [Fact]
    public async Task HandleAsync_BuyTransactionWithPrice_CreatesPriceWhenNoneExists()
    {
        // Arrange
        var instrumentId = 1;
        var transactionDate = new DateOnly(2024, 1, 15);
        var transactionPrice = 100.50m;

        var instrument = new Instrument
        {
            Id = instrumentId,
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Name = "Test Stock",
            Ticker = "TEST",
            CurrencyId = "GBP"
        };
        _context.Instruments.Add(instrument);

        var account = new Account
        {
            Id = 1,
            Name = "Test Account",
            Colour = "#FF0000"
        };
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = transactionPrice,
            Units = 10,
            Value = -1005m
        };

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map<TransactionWriteDto, Transaction>(dto)).Returns(transaction);

        var command = new CreateTransactionCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result > 0);

        var createdPrice = await _context.Prices
            .FirstOrDefaultAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        Assert.NotNull(createdPrice);
        Assert.Equal(transactionPrice, createdPrice.Close);
        Assert.Equal(transactionPrice, createdPrice.AdjustedClose);
        Assert.Equal("Trade", createdPrice.Source);
        Assert.False(createdPrice.IsLocked);

        _bus.Verify(b => b.Publish(It.Is<PriceChangedEvent>(e =>
            e.InstrumentId == instrumentId &&
            e.AsOfDate == transactionDate), null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SellTransactionWithPrice_CreatesPriceWhenNoneExists()
    {
        // Arrange
        var instrumentId = 1;
        var transactionDate = new DateOnly(2024, 1, 15);
        var transactionPrice = 150.75m;

        var instrument = new Instrument
        {
            Id = instrumentId,
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Name = "Test Stock",
            Ticker = "TEST",
            CurrencyId = "GBP"
        };
        _context.Instruments.Add(instrument);

        var account = new Account
        {
            Id = 1,
            Name = "Test Account",
            Colour = "#FF0000"
        };
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Sell,
            Date = transactionDate,
            Price = transactionPrice,
            Units = -10,
            Value = 1507.50m
        };

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map<TransactionWriteDto, Transaction>(dto)).Returns(transaction);

        var command = new CreateTransactionCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        var createdPrice = await _context.Prices
            .FirstOrDefaultAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        Assert.NotNull(createdPrice);
        Assert.Equal(transactionPrice, createdPrice.Close);
        Assert.Equal(transactionPrice, createdPrice.AdjustedClose);
        Assert.Equal("Trade", createdPrice.Source);
        Assert.False(createdPrice.IsLocked);
    }

    [Fact]
    public async Task HandleAsync_BuyTransactionWithExistingPrice_DoesNotCreateDuplicatePrice()
    {
        // Arrange
        var instrumentId = 1;
        var transactionDate = new DateOnly(2024, 1, 15);
        var transactionPrice = 100.50m;

        var instrument = new Instrument
        {
            Id = instrumentId,
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Name = "Test Stock",
            Ticker = "TEST",
            CurrencyId = "GBP"
        };
        _context.Instruments.Add(instrument);

        var account = new Account
        {
            Id = 1,
            Name = "Test Account",
            Colour = "#FF0000"
        };
        _context.Accounts.Add(account);

        // Add existing price
        var existingPrice = new Price
        {
            InstrumentId = instrumentId,
            AsOfDate = transactionDate,
            Close = 99.00m,
            AdjustedClose = 99.00m,
            Source = "Yahoo",
            IsLocked = false,
            RetrievedUtc = DateTime.UtcNow
        };
        _context.Prices.Add(existingPrice);

        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = transactionPrice,
            Units = 10,
            Value = -1005m
        };

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map<TransactionWriteDto, Transaction>(dto)).Returns(transaction);

        var command = new CreateTransactionCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        var priceCount = await _context.Prices
            .CountAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        Assert.Equal(1, priceCount);

        var price = await _context.Prices
            .FirstAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        // Price should still be the original one
        Assert.Equal(99.00m, price.Close);
        Assert.Equal("Yahoo", price.Source);
    }

    [Fact]
    public async Task HandleAsync_BuyTransactionWithoutPrice_DoesNotCreatePrice()
    {
        // Arrange
        var instrumentId = 1;
        var transactionDate = new DateOnly(2024, 1, 15);

        var instrument = new Instrument
        {
            Id = instrumentId,
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Name = "Test Stock",
            Ticker = "TEST",
            CurrencyId = "GBP"
        };
        _context.Instruments.Add(instrument);

        var account = new Account
        {
            Id = 1,
            Name = "Test Account",
            Colour = "#FF0000"
        };
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = null, // No price
            Units = 10,
            Value = -1000m
        };

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map<TransactionWriteDto, Transaction>(dto)).Returns(transaction);

        var command = new CreateTransactionCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        var priceExists = await _context.Prices
            .AnyAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        Assert.False(priceExists);
    }

    [Fact]
    public async Task HandleAsync_DepositTransaction_DoesNotCreatePrice()
    {
        // Arrange
        var transactionDate = new DateOnly(2024, 1, 15);

        var account = new Account
        {
            Id = 1,
            Name = "Test Account",
            Colour = "#FF0000"
        };
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync();

        var transaction = new Transaction
        {
            AccountId = account.Id,
            InstrumentId = null, // No instrument
            TransactionTypeId = TransactionType.DepositPersonal,
            Date = transactionDate,
            Price = null,
            Units = null,
            Value = 1000m
        };

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map<TransactionWriteDto, Transaction>(dto)).Returns(transaction);

        var command = new CreateTransactionCommand(dto);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        var priceExists = await _context.Prices
            .AnyAsync(p => p.AsOfDate == transactionDate);

        Assert.False(priceExists);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
