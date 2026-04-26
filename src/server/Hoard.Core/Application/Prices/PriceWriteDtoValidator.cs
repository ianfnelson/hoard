using FluentValidation;
using Hoard.Core.Application.Validation;
using Hoard.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Prices;

public class PriceWriteDtoValidator : AbstractValidator<PriceWriteDto>
{
    public PriceWriteDtoValidator(HoardContext context, TimeProvider timeProvider)
    {
        RuleFor(x => x.InstrumentId)
            .NotNull()
            .WithMessage("Instrument is required")
            .MustAsync(async (instrumentId, ct) =>
            {
                if (instrumentId == null) return true;
                return await context.Instruments.AnyAsync(i => i.Id == instrumentId, ct);
            })
            .WithMessage("Instrument does not exist");

        RuleFor(x => x.AsOfDate)
            .NotNull()
            .WithMessage("As of date is required")
            .Must(date => date == null || date <= DateOnlyHelper.TodayLocal(timeProvider))
            .WithMessage("As of date cannot be in the future");

        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required")
            .MaximumLength(50)
            .WithMessage("Source must not exceed 50 characters");

        RuleFor(x => x.Close)
            .NotNull()
            .WithMessage("Close price is required")
            .GreaterThan(0)
            .WithMessage("Close price must be greater than 0");

        RuleFor(x => x.AdjustedClose)
            .NotNull()
            .WithMessage("Adjusted close price is required")
            .GreaterThan(0)
            .WithMessage("Adjusted close price must be greater than 0");

        RuleFor(x => x.Open)
            .GreaterThan(0)
            .When(x => x.Open.HasValue)
            .WithMessage("Open price must be greater than 0");

        RuleFor(x => x.High)
            .GreaterThan(0)
            .When(x => x.High.HasValue)
            .WithMessage("High price must be greater than 0");

        RuleFor(x => x.Low)
            .GreaterThan(0)
            .When(x => x.Low.HasValue)
            .WithMessage("Low price must be greater than 0");

        RuleFor(x => x.Volume)
            .GreaterThan(0)
            .When(x => x.Volume.HasValue)
            .WithMessage("Volume must be greater than 0");

        // Unique constraint: (InstrumentId, AsOfDate)
        RuleFor(x => x)
            .CustomAsync(async (dto, validationContext, ct) =>
            {
                if (dto.InstrumentId == null || dto.AsOfDate == null)
                {
                    return;
                }

                int? currentId = null;
                if (validationContext.RootContextData.TryGetValue(ValidationExtensions.EntityIdKey, out var idValue)
                    && idValue is int parsedId)
                {
                    currentId = parsedId;
                }

                var exists = await context.Prices
                    .AnyAsync(p => p.InstrumentId == dto.InstrumentId && p.AsOfDate == dto.AsOfDate && p.Id != currentId, ct);

                if (exists)
                {
                    validationContext.AddFailure(nameof(dto.AsOfDate), "A price already exists for this instrument and date");
                }
            });
    }
}
