namespace Hoard.Core.Application.Portfolios;

public sealed class RebalanceActionDto
{
    public int AssetSubclassId { get; init; }
    public required string AssetSubclassName { get; init; }
    
    public RebalanceActionType RebalanceAction { get; init; }
    public decimal Amount { get; init; }
}