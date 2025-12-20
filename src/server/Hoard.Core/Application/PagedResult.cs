namespace Hoard.Core.Application;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalItems { get; init; }

    // TODO - change this to use integer arithmetic!
    public int TotalPages =>
        (int)Math.Ceiling(TotalItems / (double)PageSize);
}