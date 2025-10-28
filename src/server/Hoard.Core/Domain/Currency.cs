namespace Hoard.Core.Domain;

public class Currency : Entity<string>
{
    public required string Name { get; set; }
}