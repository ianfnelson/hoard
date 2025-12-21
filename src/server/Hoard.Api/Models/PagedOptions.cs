namespace Hoard.Api.Models;

public sealed class PagedOptions
{
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
}