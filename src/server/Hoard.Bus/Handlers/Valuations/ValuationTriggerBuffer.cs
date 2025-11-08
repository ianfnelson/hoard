using System.Collections.Concurrent;

namespace Hoard.Bus.Handlers.Valuations;

public interface IValuationTriggerBuffer
{
    void Add(DateOnly date);
    DateOnly[] SnapshotAndClear();
}

public sealed class ValuationTriggerBuffer : IValuationTriggerBuffer
{
    private readonly ConcurrentDictionary<DateOnly, byte> _dates = new();

    public void Add(DateOnly date) => _dates.TryAdd(date, 0);

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