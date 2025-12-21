namespace Hoard.Core.Application;

public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}