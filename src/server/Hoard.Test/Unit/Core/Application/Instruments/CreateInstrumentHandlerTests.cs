using Hoard.Core.Application;
using Hoard.Core.Application.Instruments;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class CreateInstrumentHandlerTests : IDisposable
{
    private readonly Hoard.Core.Data.HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task HandleAsync_MapsAndSavesInstrument()
    {
        // Arrange
        var dto = new InstrumentWriteDto();
        var instrument = new Instrument { Name = "Test Stock", Ticker = "TST", CurrencyId = "GBP" };
        _mapper.Setup(m => m.Map<InstrumentWriteDto, Instrument>(dto)).Returns(instrument);

        var handler = new Hoard.Core.Application.Instruments.CreateTransactionHandler(_mapper.Object, _context);

        // Act
        var id = await handler.HandleAsync(new CreateInstrumentCommand(dto));

        // Assert
        var saved = await _context.Instruments.FindAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("Test Stock", saved.Name);
    }

    public void Dispose() => _context.Dispose();
}
