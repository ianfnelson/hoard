namespace Hoard.Core.Application;

public interface ISortedQuery
{
    string SortBy { get; }
    SortDirection SortDirection { get; }
}