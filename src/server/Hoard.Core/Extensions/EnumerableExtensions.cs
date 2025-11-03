namespace Hoard.Core.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<List<T>> BatchesOf<T>(this IEnumerable<T> source, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var batch = new List<T>(size);

        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}