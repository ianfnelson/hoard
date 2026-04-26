using FluentValidation;
using Hoard.Core.Application.Prices;
using Hoard.Core.Application.Validation;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Prices;

public class PriceWriteDtoValidatorTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly PriceWriteDtoValidator _validator;

    public PriceWriteDtoValidatorTests()
    {
        _context = TestContextFactory.Create();
        TestData.SeedReferenceData(_context);
        _context.Instruments.Add(TestData.Instrument());
        _context.SaveChanges();
        _validator = new PriceWriteDtoValidator(_context, TimeProvider.System);
    }

    private static PriceWriteDto ValidDto() => new()
    {
        InstrumentId = 1,
        AsOfDate = new DateOnly(2024, 1, 15),
        Source = "Yahoo",
        Close = 99.50m,
        AdjustedClose = 99.50m
    };

    [Fact]
    public async Task ValidPrice_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidDto());
        Assert.True(result.IsValid);
    }

    // --- InstrumentId ---

    [Fact]
    public async Task InstrumentId_Null_Fails()
    {
        var dto = ValidDto();
        dto.InstrumentId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentId" && e.ErrorMessage == "Instrument is required");
    }

    [Fact]
    public async Task InstrumentId_NonExistent_Fails()
    {
        var dto = ValidDto();
        dto.InstrumentId = 999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentId" && e.ErrorMessage == "Instrument does not exist");
    }

    // --- AsOfDate ---

    [Fact]
    public async Task AsOfDate_Null_Fails()
    {
        var dto = ValidDto();
        dto.AsOfDate = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AsOfDate" && e.ErrorMessage == "As of date is required");
    }

    [Fact]
    public async Task AsOfDate_InFuture_Fails()
    {
        var dto = ValidDto();
        dto.AsOfDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AsOfDate" && e.ErrorMessage == "As of date cannot be in the future");
    }

    [Fact]
    public async Task AsOfDate_Today_Passes()
    {
        var dto = ValidDto();
        dto.AsOfDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "AsOfDate");
    }

    // --- Source ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Source_Empty_Fails(string? source)
    {
        var dto = ValidDto();
        dto.Source = source;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Source" && e.ErrorMessage == "Source is required");
    }

    [Fact]
    public async Task Source_TooLong_Fails()
    {
        var dto = ValidDto();
        dto.Source = new string('A', 51);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Source" && e.ErrorMessage == "Source must not exceed 50 characters");
    }

    [Fact]
    public async Task Source_ExactlyMaxLength_Passes()
    {
        var dto = ValidDto();
        dto.Source = new string('A', 50);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Source");
    }

    // --- Close ---

    [Fact]
    public async Task Close_Null_Fails()
    {
        var dto = ValidDto();
        dto.Close = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Close" && e.ErrorMessage == "Close price is required");
    }

    [Fact]
    public async Task Close_Zero_Fails()
    {
        var dto = ValidDto();
        dto.Close = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Close" && e.ErrorMessage == "Close price must be greater than 0");
    }

    [Fact]
    public async Task Close_Negative_Fails()
    {
        var dto = ValidDto();
        dto.Close = -1;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Close" && e.ErrorMessage == "Close price must be greater than 0");
    }

    // --- AdjustedClose ---

    [Fact]
    public async Task AdjustedClose_Null_Fails()
    {
        var dto = ValidDto();
        dto.AdjustedClose = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AdjustedClose" && e.ErrorMessage == "Adjusted close price is required");
    }

    [Fact]
    public async Task AdjustedClose_Zero_Fails()
    {
        var dto = ValidDto();
        dto.AdjustedClose = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AdjustedClose" && e.ErrorMessage == "Adjusted close price must be greater than 0");
    }

    // --- Optional price fields ---

    [Theory]
    [InlineData(nameof(PriceWriteDto.Open), "Open price must be greater than 0")]
    [InlineData(nameof(PriceWriteDto.High), "High price must be greater than 0")]
    [InlineData(nameof(PriceWriteDto.Low), "Low price must be greater than 0")]
    public async Task OptionalPriceField_Zero_Fails(string propertyName, string expectedMessage)
    {
        var dto = ValidDto();
        typeof(PriceWriteDto).GetProperty(propertyName)!.SetValue(dto, 0m);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName && e.ErrorMessage == expectedMessage);
    }

    [Theory]
    [InlineData(nameof(PriceWriteDto.Open))]
    [InlineData(nameof(PriceWriteDto.High))]
    [InlineData(nameof(PriceWriteDto.Low))]
    public async Task OptionalPriceField_Positive_Passes(string propertyName)
    {
        var dto = ValidDto();
        typeof(PriceWriteDto).GetProperty(propertyName)!.SetValue(dto, 50m);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == propertyName);
    }

    [Theory]
    [InlineData(nameof(PriceWriteDto.Open))]
    [InlineData(nameof(PriceWriteDto.High))]
    [InlineData(nameof(PriceWriteDto.Low))]
    public async Task OptionalPriceField_Null_Passes(string propertyName)
    {
        var dto = ValidDto();
        typeof(PriceWriteDto).GetProperty(propertyName)!.SetValue(dto, (decimal?)null);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == propertyName);
    }

    // --- Volume ---

    [Fact]
    public async Task Volume_Zero_Fails()
    {
        var dto = ValidDto();
        dto.Volume = 0;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Volume" && e.ErrorMessage == "Volume must be greater than 0");
    }

    [Fact]
    public async Task Volume_Positive_Passes()
    {
        var dto = ValidDto();
        dto.Volume = 1000;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Volume");
    }

    [Fact]
    public async Task Volume_Null_Passes()
    {
        var dto = ValidDto();
        dto.Volume = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Volume");
    }

    // --- Unique constraint (InstrumentId, AsOfDate) ---

    [Fact]
    public async Task DuplicateInstrumentAndDate_Fails()
    {
        _context.Prices.Add(TestData.Price(instrumentId: 1, asOfDate: new DateOnly(2024, 1, 15)));
        await _context.SaveChangesAsync();

        var dto = ValidDto();
        dto.InstrumentId = 1;
        dto.AsOfDate = new DateOnly(2024, 1, 15);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AsOfDate" && e.ErrorMessage == "A price already exists for this instrument and date");
    }

    [Fact]
    public async Task DuplicateInstrumentAndDate_ExcludesCurrentEntity_OnUpdate()
    {
        var existingPrice = TestData.Price(instrumentId: 1, asOfDate: new DateOnly(2024, 1, 15));
        _context.Prices.Add(existingPrice);
        await _context.SaveChangesAsync();

        var dto = ValidDto();
        dto.InstrumentId = 1;
        dto.AsOfDate = new DateOnly(2024, 1, 15);

        var validationContext = new ValidationContext<PriceWriteDto>(dto);
        validationContext.RootContextData[ValidationExtensions.EntityIdKey] = existingPrice.Id;

        var result = await _validator.ValidateAsync(validationContext);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "AsOfDate" && e.ErrorMessage == "A price already exists for this instrument and date");
    }

    [Fact]
    public async Task DifferentInstrument_SameDate_Passes()
    {
        _context.Prices.Add(TestData.Price(instrumentId: 1, asOfDate: new DateOnly(2024, 1, 15)));
        _context.Instruments.Add(TestData.Instrument(id: 2, name: "Other", ticker: "OTHER"));
        await _context.SaveChangesAsync();

        var dto = ValidDto();
        dto.InstrumentId = 2;
        dto.AsOfDate = new DateOnly(2024, 1, 15);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "AsOfDate");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
