namespace Hoard.Core.Extensions;

public static class ListExtensions
{
    public static List<T> Shuffle<T>(this IList<T> list)
    {
        return list.OrderBy(x => Guid.NewGuid()).ToList();
    }
}