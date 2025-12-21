namespace Hoard.Core.Application.Instruments;

public sealed class GetInstrumentsQuery
    : IQuery<PagedResult<InstrumentSummaryDto>>,
        IPagedQuery,
        ISortedQuery
{
    public string? Search { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;

    public string SortBy { get; init; } = "name";
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}