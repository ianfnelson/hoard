using FluentValidation;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Portfolios;

public class SetTargetAllocationsRequestValidator : AbstractValidator<SetTargetAllocationsRequest>
{
    public SetTargetAllocationsRequestValidator(HoardContext context)
    {
        RuleForEach(x => x.Allocations).ChildRules(item =>
        {
            item.RuleFor(i => i.Target)
                .InclusiveBetween(0m, 100m)
                .WithMessage("Target must be between 0 and 100");
        });

        RuleFor(x => x.Allocations)
            .Must(allocations => allocations.All(a => a.AssetSubclassId != AssetSubclass.Cash))
            .WithMessage("Cash allocation is derived from the remainder and must not be provided");

        RuleFor(x => x.Allocations)
            .Must(allocations =>
                allocations.Select(a => a.AssetSubclassId).Distinct().Count() == allocations.Count)
            .WithMessage("Duplicate asset subclass in allocations");

        RuleFor(x => x.Allocations)
            .Must(allocations => allocations.Sum(a => a.Target) <= 100m)
            .WithMessage("Target allocations must not exceed 100%");

        RuleFor(x => x.Allocations)
            .MustAsync(async (allocations, ct) =>
            {
                var ids = allocations.Select(a => a.AssetSubclassId).Distinct().ToList();
                if (ids.Count == 0)
                {
                    return true;
                }

                var existingCount = await context.AssetSubclasses
                    .CountAsync(s => ids.Contains(s.Id), ct);
                return existingCount == ids.Count;
            })
            .WithMessage("One or more asset subclasses do not exist");
    }
}
