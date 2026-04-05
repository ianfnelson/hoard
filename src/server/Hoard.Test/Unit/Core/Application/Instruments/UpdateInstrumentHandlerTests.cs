using Hoard.Core.Application;
using Hoard.Core.Application.Instruments;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Moq;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class UpdateInstrumentHandlerTests : IDisposable
{
    private readonly Hoard.Core.Data.HoardContext _context = TestContextFactory.Create();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public async Task HandleAsync_UpdatesExistingInstrument()
    {
        // Arrange
        var instrument = TestData.Instrument();
        _context.Instruments.Add(instrument);
        await _context.SaveChangesAsync();

        var dto = new InstrumentWriteDto();
        _mapper.Setup(m => m.Map(dto, It.IsAny<Instrument>()));

        var handler = new UpdateInstrumentHandler(_context, _mapper.Object);

        // Act
        await handler.HandleAsync(new UpdateInstrumentCommand(1, dto));

        // Assert
        _mapper.Verify(m => m.Map(dto, It.IsAny<Instrument>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_Throws()
    {
        var handler = new UpdateInstrumentHandler(_context, _mapper.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(new UpdateInstrumentCommand(999, new InstrumentWriteDto())));
    }

    public void Dispose() => _context.Dispose();
}
