using Hoard.Core.Application;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Messages.Prices;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class UpdateTransactionHandlerTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus;
    private readonly Mock<IMapper> _mapper;
    private readonly UpdateTransactionHandler _handler;

    public UpdateTransactionHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HoardContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new HoardContext(options);

        _bus = new Mock<IBus>();
        _mapper = new Mock<IMapper>();
        _handler = new UpdateTransactionHandler(_context, _mapper.Object, _bus.Object);
    }

    [Fact]
    public async Task HandleAsync_UpdatedToBuyWithPrice_CreatesPriceWhenNoneExists()
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

        var transaction = new Transaction
        {
            Id = 1,
            AccountId = account.Id,
            InstrumentId = null, // Initially no instrument
            TransactionTypeId = TransactionType.DepositPersonal,
            Date = transactionDate,
            Price = null,
            Units = null,
            Value = 1000m
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Transaction>()))
            .Callback((TransactionWriteDto d, Transaction tx) =>
            {
                tx.InstrumentId = instrumentId;
                tx.TransactionTypeId = TransactionType.Buy;
                tx.Price = transactionPrice;
                tx.Units = 10;
                tx.Value = -1005m;
            });

        var command = new UpdateTransactionCommand(transaction.Id, dto);

        // Act
        await _handler.HandleAsync(command);

        // Assert
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
    public async Task HandleAsync_UpdatedToSellWithPrice_CreatesPriceWhenNoneExists()
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

        var transaction = new Transaction
        {
            Id = 1,
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = 100m,
            Units = 10,
            Value = -1000m
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Transaction>()))
            .Callback((TransactionWriteDto d, Transaction tx) =>
            {
                tx.TransactionTypeId = TransactionType.Sell;
                tx.Price = transactionPrice;
                tx.Units = -10;
                tx.Value = 1507.50m;
            });

        var command = new UpdateTransactionCommand(transaction.Id, dto);

        // Act
        await _handler.HandleAsync(command);

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
    public async Task HandleAsync_UpdatedWithExistingPrice_DoesNotCreateDuplicatePrice()
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

        var transaction = new Transaction
        {
            Id = 1,
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = transactionPrice,
            Units = 10,
            Value = -1005m
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Transaction>()))
            .Callback((TransactionWriteDto d, Transaction tx) =>
            {
                tx.Price = transactionPrice + 5; // Change price
            });

        var command = new UpdateTransactionCommand(transaction.Id, dto);

        // Act
        await _handler.HandleAsync(command);

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
    public async Task HandleAsync_UpdatedBuyWithoutPrice_DoesNotCreatePrice()
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

        var transaction = new Transaction
        {
            Id = 1,
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = 100m,
            Units = 10,
            Value = -1000m
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Transaction>()))
            .Callback((TransactionWriteDto d, Transaction tx) =>
            {
                tx.Price = null; // Remove price
            });

        var command = new UpdateTransactionCommand(transaction.Id, dto);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var priceExists = await _context.Prices
            .AnyAsync(p => p.InstrumentId == instrumentId && p.AsOfDate == transactionDate);

        Assert.False(priceExists);
    }

    [Fact]
    public async Task HandleAsync_UpdatedToDeposit_DoesNotCreatePrice()
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

        var transaction = new Transaction
        {
            Id = 1,
            AccountId = account.Id,
            InstrumentId = instrumentId,
            TransactionTypeId = TransactionType.Buy,
            Date = transactionDate,
            Price = 100m,
            Units = 10,
            Value = -1000m
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var dto = new TransactionWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Transaction>()))
            .Callback((TransactionWriteDto d, Transaction tx) =>
            {
                tx.InstrumentId = null;
                tx.TransactionTypeId = TransactionType.DepositPersonal;
                tx.Price = null;
                tx.Units = null;
                tx.Value = 1000m;
            });

        var command = new UpdateTransactionCommand(transaction.Id, dto);

        // Act
        await _handler.HandleAsync(command);

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
