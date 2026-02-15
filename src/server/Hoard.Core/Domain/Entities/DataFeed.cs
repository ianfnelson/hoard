namespace Hoard.Core.Domain.Entities;

public class DataFeed : Entity<int>
{
    public required string Name { get; set; }

    public const int News = 1;
    public const int Prices = 2;
    public const int Quotes = 3;
}
