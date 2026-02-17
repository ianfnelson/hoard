namespace Hoard.Core.Domain.Entities;

public partial class DataFeed : Entity<int>
{
    public required string Name { get; set; }
}
