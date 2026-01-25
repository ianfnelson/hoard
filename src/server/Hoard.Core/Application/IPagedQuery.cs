namespace Hoard.Core.Application;

public interface IPagedQuery
{
    int PageNumber { get; }
    int PageSize { get; }
}