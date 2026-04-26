using FluentValidation;
using Hoard.Core.Application.Transactions;
using Hoard.Core.Application.Validation;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Transactions;

public class TransactionWriteDtoValidatorTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly TransactionWriteDtoValidator _validator;

    public TransactionWriteDtoValidatorTests()
    {
        _context = TestContextFactory.Create();
        TestData.SeedReferenceData(_context);
        _context.Accounts.Add(TestData.Account());
        _context.Instruments.Add(TestData.Instrument());
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.Sell, "Sell"));
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.CorporateAction, "Corporate Action"));
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.IncomeDividend, "Income Dividend"));
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.IncomeLoyaltyBonus, "Income Loyalty Bonus"));
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.DepositPersonal, "Deposit Personal"));
        _context.TransactionTypes.Add(TestData.TransactionType(TransactionType.Fee, "Fee"));
        _context.SaveChanges();
        _validator = new TransactionWriteDtoValidator(_context, TimeProvider.System);
    }

    private static TransactionWriteDto ValidBuyDto() => new()
    {
        AccountId = 1,
        InstrumentId = 1,
        TransactionTypeId = TransactionType.Buy,
        Date = new DateOnly(2024, 1, 15),
        Value = 1005m,
        Units = 10,
        Price = 100.50m
    };

    [Fact]
    public async Task ValidBuyTransaction_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidBuyDto());
        Assert.True(result.IsValid);
    }

    // --- AccountId ---

    [Fact]
    public async Task AccountId_Null_Fails()
    {
        var dto = ValidBuyDto();
        dto.AccountId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AccountId" && e.ErrorMessage == "AccountId is required");
    }

    [Fact]
    public async Task AccountId_NonExistent_Fails()
    {
        var dto = ValidBuyDto();
        dto.AccountId = 999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AccountId" && e.ErrorMessage == "Account with ID 999 does not exist");
    }

    // --- TransactionTypeId ---

    [Fact]
    public async Task TransactionTypeId_Null_Fails()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "TransactionTypeId" && e.ErrorMessage == "TransactionTypeId is required");
    }

    [Fact]
    public async Task TransactionTypeId_NonExistent_Fails()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = 9999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "TransactionTypeId" && e.ErrorMessage == "TransactionType with ID 9999 does not exist");
    }

    // --- Date ---

    [Fact]
    public async Task Date_Null_Fails()
    {
        var dto = ValidBuyDto();
        dto.Date = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date" && e.ErrorMessage == "Date is required");
    }

    [Fact]
    public async Task Date_InFuture_Fails()
    {
        var dto = ValidBuyDto();
        dto.Date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date" && e.ErrorMessage == "Transaction date cannot be in the future");
    }

    [Fact]
    public async Task Date_Today_Passes()
    {
        var dto = ValidBuyDto();
        dto.Date = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Date");
    }

    // --- Value ---

    [Fact]
    public async Task Value_Null_Fails()
    {
        var dto = ValidBuyDto();
        dto.Value = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Value" && e.ErrorMessage == "Value is required");
    }

    [Fact]
    public async Task Value_Zero_ForNonCorporateAction_Fails()
    {
        var dto = ValidBuyDto();
        dto.Value = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Value" && e.ErrorMessage == "Value must be greater than zero");
    }

    [Fact]
    public async Task Value_Zero_ForCorporateAction_Passes()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = TransactionType.CorporateAction;
        dto.Value = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Value" && e.ErrorMessage == "Value must be greater than zero");
    }

    // --- InstrumentId conditional rules ---

    [Theory]
    [InlineData(TransactionType.Buy)]
    [InlineData(TransactionType.Sell)]
    [InlineData(TransactionType.CorporateAction)]
    [InlineData(TransactionType.IncomeDividend)]
    public async Task InstrumentId_Null_WhenRequired_Fails(int transactionTypeId)
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = transactionTypeId;
        dto.InstrumentId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentId" && e.ErrorMessage.StartsWith("InstrumentId is required"));
    }

    [Fact]
    public async Task InstrumentId_NonExistent_Fails()
    {
        var dto = ValidBuyDto();
        dto.InstrumentId = 999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentId" && e.ErrorMessage == "Instrument with ID 999 does not exist");
    }

    [Fact]
    public async Task InstrumentId_Provided_ForLoyaltyBonus_Passes()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = TransactionType.IncomeLoyaltyBonus;
        dto.InstrumentId = 1;
        dto.Units = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "InstrumentId");
    }

    [Fact]
    public async Task InstrumentId_Null_ForLoyaltyBonus_Passes()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = TransactionType.IncomeLoyaltyBonus;
        dto.InstrumentId = null;
        dto.Units = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "InstrumentId");
    }

    [Theory]
    [InlineData(TransactionType.DepositPersonal)]
    [InlineData(TransactionType.Fee)]
    [InlineData(TransactionType.Withdrawal)]
    public async Task InstrumentId_Provided_WhenForbidden_Fails(int transactionTypeId)
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = transactionTypeId;
        dto.InstrumentId = 1;
        dto.Units = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentId" && e.ErrorMessage.Contains("should not be provided"));
    }

    [Fact]
    public async Task InstrumentId_Null_ForDeposit_Passes()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = TransactionType.DepositPersonal;
        dto.InstrumentId = null;
        dto.Units = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "InstrumentId");
    }

    // --- Units conditional rules ---

    [Theory]
    [InlineData(TransactionType.Buy)]
    [InlineData(TransactionType.Sell)]
    [InlineData(TransactionType.CorporateAction)]
    public async Task Units_Null_WhenRequired_Fails(int transactionTypeId)
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = transactionTypeId;
        dto.Units = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Units" && e.ErrorMessage.StartsWith("Units is required"));
    }

    [Fact]
    public async Task Units_Positive_ForBuy_Passes()
    {
        var dto = ValidBuyDto();
        dto.Units = 5;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Units");
    }

    [Fact]
    public async Task Units_Zero_ForNonCorporateAction_Fails()
    {
        var dto = ValidBuyDto();
        dto.Units = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Units" && e.ErrorMessage == "Units must be greater than zero");
    }

    [Fact]
    public async Task Units_Negative_ForCorporateAction_Passes()
    {
        var dto = ValidBuyDto();
        dto.TransactionTypeId = TransactionType.CorporateAction;
        dto.Units = -5;
        dto.Value = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Units" && e.ErrorMessage == "Units must be greater than zero");
    }

    // --- ContractNoteReference ---

    [Fact]
    public async Task ContractNoteReference_TooLong_Fails()
    {
        var dto = ValidBuyDto();
        dto.ContractNoteReference = new string('A', 21);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "ContractNoteReference" && e.ErrorMessage == "ContractNoteReference cannot exceed 20 characters");
    }

    [Fact]
    public async Task ContractNoteReference_ExactlyMaxLength_Passes()
    {
        var dto = ValidBuyDto();
        dto.ContractNoteReference = new string('A', 20);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "ContractNoteReference");
    }

    // --- Price ---

    [Fact]
    public async Task Price_Zero_Fails()
    {
        var dto = ValidBuyDto();
        dto.Price = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price" && e.ErrorMessage == "Price must be greater than zero");
    }

    [Fact]
    public async Task Price_Negative_Fails()
    {
        var dto = ValidBuyDto();
        dto.Price = -1;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price" && e.ErrorMessage == "Price must be greater than zero");
    }

    [Fact]
    public async Task Price_Null_Passes()
    {
        var dto = ValidBuyDto();
        dto.Price = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Price");
    }

    // --- Charge fields ---

    [Theory]
    [InlineData(nameof(TransactionWriteDto.DealingCharge), "DealingCharge cannot be negative")]
    [InlineData(nameof(TransactionWriteDto.StampDuty), "StampDuty cannot be negative")]
    [InlineData(nameof(TransactionWriteDto.PtmLevy), "PtmLevy cannot be negative")]
    [InlineData(nameof(TransactionWriteDto.FxCharge), "FxCharge cannot be negative")]
    public async Task ChargeField_Negative_Fails(string propertyName, string expectedMessage)
    {
        var dto = ValidBuyDto();
        typeof(TransactionWriteDto).GetProperty(propertyName)!.SetValue(dto, -1m);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData(nameof(TransactionWriteDto.DealingCharge))]
    [InlineData(nameof(TransactionWriteDto.StampDuty))]
    [InlineData(nameof(TransactionWriteDto.PtmLevy))]
    [InlineData(nameof(TransactionWriteDto.FxCharge))]
    public async Task ChargeField_Zero_Passes(string propertyName)
    {
        var dto = ValidBuyDto();
        typeof(TransactionWriteDto).GetProperty(propertyName)!.SetValue(dto, 0m);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == propertyName);
    }

    [Theory]
    [InlineData(nameof(TransactionWriteDto.DealingCharge))]
    [InlineData(nameof(TransactionWriteDto.StampDuty))]
    [InlineData(nameof(TransactionWriteDto.PtmLevy))]
    [InlineData(nameof(TransactionWriteDto.FxCharge))]
    public async Task ChargeField_Null_Passes(string propertyName)
    {
        var dto = ValidBuyDto();
        typeof(TransactionWriteDto).GetProperty(propertyName)!.SetValue(dto, (decimal?)null);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == propertyName);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
