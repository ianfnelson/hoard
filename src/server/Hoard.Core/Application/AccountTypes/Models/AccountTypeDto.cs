namespace Hoard.Core.Application.AccountTypes.Models;

public class AccountTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedUtc { get; set; }
}