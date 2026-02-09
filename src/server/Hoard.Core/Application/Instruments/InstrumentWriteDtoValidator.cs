using FluentValidation;
using Hoard.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Instruments;

public class InstrumentWriteDtoValidator : AbstractValidator<InstrumentWriteDto>
{
    public InstrumentWriteDtoValidator(HoardContext context, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.InstrumentTypeId)
            .NotNull()
            .WithMessage("InstrumentTypeId is required")
            .MustAsync(async(id, ct) => await context.InstrumentTypes.AnyAsync(x => x.Id == id, ct))
            .WithMessage(x => $"InstrumentType with ID {x.InstrumentTypeId} does not exist");

        RuleFor(x => x.AssetSubclassId)
            .NotNull()
            .WithMessage("AssetSubclassId is required")
            .MustAsync(async(id, ct) => await context.AssetSubclasses.AnyAsync(x => x.Id == id, ct))
            .WithMessage(x => $"AssetSubclass with ID {x.AssetSubclassId} does not exist");

        RuleFor(x => x.CurrencyId)
            .NotNull()
            .WithMessage("CurrencyId is required")
            .MustAsync(async(id, ct) => await context.Currencies.AnyAsync(x => x.Id == id, ct))
            .WithMessage(x => $"Currency with ID {x.CurrencyId} does not exist");

        RuleFor(x => x.TickerDisplay)
            .NotEmpty()
            .WithMessage("TickerDisplay is required")
            .MaximumLength(20)
            .WithMessage("TickerDisplay cannot exceed 20 characters");

        RuleFor(x => x.Isin)
            .MaximumLength(12)
            .WithMessage("ISIN cannot exceed 12 characters")
            .MustAsync(async (isin, ct) =>
            {
                if (isin == null)
                {
                    return true;
                }

                int? currentId = null;
                if (httpContextAccessor.HttpContext?.Request.RouteValues.TryGetValue("id", out var idValue) == true
                    && int.TryParse(idValue?.ToString(), out var parsedId))
                {
                    currentId = parsedId;
                }

                return !await context.Instruments
                    .AnyAsync(i => i.Isin == isin && i.Id != currentId, ct);
            })
            .WithMessage("An instrument with this ISIN already exists");
    }
}