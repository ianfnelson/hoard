using Hoard.Core.Application.Alerts;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;
using Rebus.Bus;

namespace Hoard.Test.Unit.Core.Application.Alerts;

public class AlertServiceTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly Mock<IBus> _bus;
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _context = TestContextFactory.Create();
        _bus = new Mock<IBus>();
        _sut = new AlertService(_context, _bus.Object);
    }

    [Fact]
    public async Task TryRaiseAsync_FirstAlert_ReturnsTrue()
    {
        var request = MakeRequest("key-1");

        var result = await _sut.TryRaiseAsync(request);

        Assert.True(result);
        Assert.Equal(1, await _context.Alerts.CountAsync());
    }

    [Fact]
    public async Task TryRaiseAsync_DuplicateSuppressionKey_ReturnsFalse()
    {
        var request = MakeRequest("duplicate-key");
        await _sut.TryRaiseAsync(request);

        var result = await _sut.TryRaiseAsync(request);

        Assert.False(result);
        Assert.Equal(1, await _context.Alerts.CountAsync());
    }

    [Fact]
    public async Task TryRaiseAsync_DuplicateSuppressionKey_DoesNotPublishEvent()
    {
        var request = MakeRequest("dup-no-publish");
        await _sut.TryRaiseAsync(request);

        await _sut.TryRaiseAsync(request);

        _bus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task TryRaiseAsync_DifferentSuppressionKeys_BothSucceed()
    {
        var result1 = await _sut.TryRaiseAsync(MakeRequest("key-a"));
        var result2 = await _sut.TryRaiseAsync(MakeRequest("key-b"));

        Assert.True(result1);
        Assert.True(result2);
        Assert.Equal(2, await _context.Alerts.CountAsync());
    }

    private static AlertRequest MakeRequest(string suppressionKey) => new()
    {
        AlertTypeId = AlertType.QuoteMovement,
        TriggeredDate = new DateOnly(2026, 4, 3),
        InstrumentId = 1,
        SuppressionKey = suppressionKey,
        Title = "Test alert",
        Message = "Test message"
    };

    public void Dispose() => _context.Dispose();
}
