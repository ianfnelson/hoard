using System.Collections.Concurrent;
using Hoard.Core;

namespace Hoard.Bus.Handlers.Holdings;

public interface IHoldingTriggerBuffer
{
    void AddDatesFrom(DateOnly date);
    DateOnly[] SnapshotAndClear();
}

public sealed class HoldingTriggerBuffer : IHoldingTriggerBuffer
{
    private readonly ConcurrentDictionary<DateOnly, byte> _dates = new();
    
    public void AddDatesFrom(DateOnly date)
    {
        var today = DateOnlyHelper.TodayLocal();

        for (var d = date; d <= today; d = d.AddDays(1))
        {
            _dates.TryAdd(d, 0);
        }
    }

    public DateOnly[] SnapshotAndClear()
    {
        var snapshot = _dates.Keys.ToArray();
        
        foreach (var d in snapshot)
        {
            _dates.TryRemove(d, out _);
        }
        
        return snapshot;
    }
}