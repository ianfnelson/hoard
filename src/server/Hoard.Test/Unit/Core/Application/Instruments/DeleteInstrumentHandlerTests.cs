using Hoard.Core.Application.Instruments;
using Hoard.Core.Data;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class DeleteInstrumentHandlerTests : IDisposable
{
    private readonly HoardContext _context = TestContextFactory.Create();

    [Fact]
    public async Task HandleAsync_RemovesInstrumentFromDatabase()
    {
        // Arrange
        _context.Instruments.Add(TestData.Instrument());
        await _context.SaveChangesAsync();

        var handler = new DeleteInstrumentHandler(_context);

        // Act
        await handler.HandleAsync(new DeleteInstrumentCommand(1));

        // Assert
        var count = await _context.Instruments.CountAsync();
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task HandleAsync_NonExistentInstrument_Throws()
    {
        var handler = new DeleteInstrumentHandler(_context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(new DeleteInstrumentCommand(999)));
    }

    public void Dispose() => _context.Dispose();
}
