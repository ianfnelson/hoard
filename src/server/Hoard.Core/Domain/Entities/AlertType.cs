namespace Hoard.Core.Domain.Entities;

public partial class AlertType : Entity<int>
{
    public required string Name { get; set; }
}