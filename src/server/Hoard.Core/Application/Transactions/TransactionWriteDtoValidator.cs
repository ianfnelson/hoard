using FluentValidation;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hoard.Core.Application.Transactions;

public class TransactionWriteDtoValidator : AbstractValidator<TransactionWriteDto>
{
    // Transaction types requiring instrument
    private static readonly int[] InstrumentRequiredTypes =
    [
        TransactionType.Buy,
        TransactionType.Sell,
        TransactionType.CorporateAction,
        TransactionType.IncomeDividend
    ];

    // Transaction types allowing optional instrument
    private static readonly int[] InstrumentOptionalTypes =
    [
        TransactionType.IncomeLoyaltyBonus
    ];

    public TransactionWriteDtoValidator(HoardContext context)
    {
        // Required fields
        RuleFor(x => x.AccountId)
            .NotNull()
            .WithMessage("AccountId is required")
            .MustAsync(async (id, ct) => await context.Accounts.AnyAsync(a => a.Id == id, ct))
            .WithMessage(x => $"Account with ID {x.AccountId} does not exist");

        RuleFor(x => x.TransactionTypeId)
            .NotNull()
            .WithMessage("TransactionTypeId is required")
            .MustAsync(async (id, ct) => await context.TransactionTypes.AnyAsync(tt => tt.Id == id, ct))
            .WithMessage(x => $"TransactionType with ID {x.TransactionTypeId} does not exist");

        RuleFor(x => x.Date)
            .NotNull()
            .WithMessage("Date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Value)
            .NotNull()
            .WithMessage("Value is required")
            .GreaterThan(0)
            .When(x => x.TransactionTypeId != TransactionType.CorporateAction)
            .WithMessage("Value must be greater than zero");

        // Conditional instrument validation
        RuleFor(x => x.InstrumentId)
            .NotNull()
            .When(x => x.TransactionTypeId.HasValue && InstrumentRequiredTypes.Contains(x.TransactionTypeId.Value))
            .WithMessage(x => $"InstrumentId is required for transaction type {x.TransactionTypeId}")
            .MustAsync(async (id, ct) => id == null || await context.Instruments.AnyAsync(i => i.Id == id, ct))
            .WithMessage(x => $"Instrument with ID {x.InstrumentId} does not exist");

        RuleFor(x => x.InstrumentId)
            .Null()
            .When(x => x.TransactionTypeId.HasValue &&
                       !InstrumentRequiredTypes.Contains(x.TransactionTypeId.Value) &&
                       !InstrumentOptionalTypes.Contains(x.TransactionTypeId.Value))
            .WithMessage(x => $"InstrumentId should not be provided for transaction type {x.TransactionTypeId}");

        // String length constraints
        RuleFor(x => x.ContractNoteReference)
            .MaximumLength(20)
            .WithMessage("ContractNoteReference cannot exceed 20 characters");

        // Numeric constraints
        RuleFor(x => x.Units)
            .GreaterThan(0)
            .When(x => x.Units.HasValue && x.TransactionTypeId != TransactionType.CorporateAction)
            .WithMessage("Units must be greater than zero");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.DealingCharge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DealingCharge.HasValue)
            .WithMessage("DealingCharge cannot be negative");

        RuleFor(x => x.StampDuty)
            .GreaterThanOrEqualTo(0)
            .When(x => x.StampDuty.HasValue)
            .WithMessage("StampDuty cannot be negative");

        RuleFor(x => x.PtmLevy)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PtmLevy.HasValue)
            .WithMessage("PtmLevy cannot be negative");

        RuleFor(x => x.FxCharge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FxCharge.HasValue)
            .WithMessage("FxCharge cannot be negative");
    }
}
