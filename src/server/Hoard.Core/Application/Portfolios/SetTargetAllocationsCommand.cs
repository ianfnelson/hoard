using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public record SetTargetAllocationsCommand(int PortfolioId, SetTargetAllocationsRequest Request) : ICommand;

public class SetTargetAllocationsHandler(HoardContext context)
    : ICommandHandler<SetTargetAllocationsCommand>
{
    public async Task HandleAsync(SetTargetAllocationsCommand command, CancellationToken ct = default)
    {
        var (portfolioId, request) = command;

        var portfolioExists = await context.Portfolios
            .AnyAsync(p => p.Id == portfolioId, ct);

        if (!portfolioExists)
        {
            throw new InvalidOperationException($"Portfolio {portfolioId} not found.");
        }

        var existing = await context.TargetAllocations
            .Where(ta => ta.PortfolioId == portfolioId)
            .ToListAsync(ct);

        context.TargetAllocations.RemoveRange(existing);

        var allocations = request.Allocations
            .Where(a => a.Target > 0m)
            .Select(a => new TargetAllocation
            {
                PortfolioId = portfolioId,
                AssetSubclassId = a.AssetSubclassId,
                Target = a.Target
            })
            .ToList();

        var cashRemainder = 100m - request.Allocations.Sum(a => a.Target);
        if (cashRemainder > 0m)
        {
            allocations.Add(new TargetAllocation
            {
                PortfolioId = portfolioId,
                AssetSubclassId = AssetSubclass.Cash,
                Target = cashRemainder
            });
        }

        context.TargetAllocations.AddRange(allocations);

        await context.SaveChangesAsync(ct);
    }
}
