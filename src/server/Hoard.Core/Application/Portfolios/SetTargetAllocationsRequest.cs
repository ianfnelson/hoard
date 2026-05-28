namespace Hoard.Core.Application.Portfolios;

public record SetTargetAllocationsRequest
{
    public List<TargetAllocationItemDto> Allocations { get; init; } = [];
}

public record TargetAllocationItemDto
{
    public int AssetSubclassId { get; init; }
    public decimal Target { get; init; }
}
