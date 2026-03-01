namespace Hoard.Core.Application;

public interface ISortedQuery
{
    string SortBy { get; }
    string SortDirection { get; }
}