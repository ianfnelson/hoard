using FluentValidation;
using Hoard.Core.Application.Instruments;
using Hoard.Core.Application.Validation;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Instruments;

public class InstrumentWriteDtoValidatorTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly InstrumentWriteDtoValidator _validator;

    public InstrumentWriteDtoValidatorTests()
    {
        _context = TestContextFactory.Create();
        TestData.SeedReferenceData(_context);
        _context.DataFeeds.Add(new DataFeed { Id = 1, Name = "Yahoo" });
        _context.DataFeeds.Add(new DataFeed { Id = 2, Name = "Manual" });
        _context.SaveChanges();
        _validator = new InstrumentWriteDtoValidator(_context);
    }

    private static InstrumentWriteDto ValidDto() => new()
    {
        Name = "Test Instrument",
        Ticker = "TEST",
        InstrumentTypeId = 1,
        AssetSubclassId = 1,
        CurrencyId = "GBP"
    };

    [Fact]
    public async Task ValidInstrument_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidDto());
        Assert.True(result.IsValid);
    }

    // --- Name ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Name_Empty_Fails(string? name)
    {
        var dto = ValidDto();
        dto.Name = name;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
    }

    [Fact]
    public async Task Name_TooLong_Fails()
    {
        var dto = ValidDto();
        dto.Name = new string('A', 101);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name cannot exceed 100 characters");
    }

    [Fact]
    public async Task Name_ExactlyMaxLength_Passes()
    {
        var dto = ValidDto();
        dto.Name = new string('A', 100);
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Name");
    }

    // --- Ticker ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Ticker_Empty_Fails(string? ticker)
    {
        var dto = ValidDto();
        dto.Ticker = ticker;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Ticker" && e.ErrorMessage == "Ticker is required");
    }

    [Fact]
    public async Task Ticker_TooLong_Fails()
    {
        var dto = ValidDto();
        dto.Ticker = new string('A', 21);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Ticker" && e.ErrorMessage == "Ticker cannot exceed 20 characters");
    }

    // --- InstrumentTypeId ---

    [Fact]
    public async Task InstrumentTypeId_Null_Fails()
    {
        var dto = ValidDto();
        dto.InstrumentTypeId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentTypeId" && e.ErrorMessage == "InstrumentTypeId is required");
    }

    [Fact]
    public async Task InstrumentTypeId_NonExistent_Fails()
    {
        var dto = ValidDto();
        dto.InstrumentTypeId = 999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "InstrumentTypeId" && e.ErrorMessage == "InstrumentType with ID 999 does not exist");
    }

    // --- AssetSubclassId ---

    [Fact]
    public async Task AssetSubclassId_Null_Fails()
    {
        var dto = ValidDto();
        dto.AssetSubclassId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AssetSubclassId" && e.ErrorMessage == "AssetSubclassId is required");
    }

    [Fact]
    public async Task AssetSubclassId_NonExistent_Fails()
    {
        var dto = ValidDto();
        dto.AssetSubclassId = 999;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "AssetSubclassId" && e.ErrorMessage == "AssetSubclass with ID 999 does not exist");
    }

    // --- CurrencyId ---

    [Fact]
    public async Task CurrencyId_Null_Fails()
    {
        var dto = ValidDto();
        dto.CurrencyId = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "CurrencyId" && e.ErrorMessage == "CurrencyId is required");
    }

    [Fact]
    public async Task CurrencyId_NonExistent_Fails()
    {
        var dto = ValidDto();
        dto.CurrencyId = "XYZ";
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "CurrencyId" && e.ErrorMessage == "Currency with ID XYZ does not exist");
    }

    // --- ISIN ---

    [Fact]
    public async Task Isin_TooLong_Fails()
    {
        var dto = ValidDto();
        dto.Isin = new string('A', 13);
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Isin" && e.ErrorMessage == "ISIN cannot exceed 12 characters");
    }

    [Fact]
    public async Task Isin_Null_Passes()
    {
        var dto = ValidDto();
        dto.Isin = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Isin");
    }

    [Fact]
    public async Task Isin_ExactlyMaxLength_Passes()
    {
        var dto = ValidDto();
        dto.Isin = "GB0002634946";
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Isin");
    }

    [Fact]
    public async Task Isin_Duplicate_Fails()
    {
        _context.Instruments.Add(new Instrument
        {
            Id = 10,
            Name = "Existing",
            Ticker = "EX",
            CurrencyId = "GBP",
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Isin = "GB0002634946"
        });
        await _context.SaveChangesAsync();

        var dto = ValidDto();
        dto.Isin = "GB0002634946";
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Isin" && e.ErrorMessage == "An instrument with this ISIN already exists");
    }

    [Fact]
    public async Task Isin_Duplicate_ExcludesCurrentEntity_OnUpdate()
    {
        _context.Instruments.Add(new Instrument
        {
            Id = 10,
            Name = "Existing",
            Ticker = "EX",
            CurrencyId = "GBP",
            InstrumentTypeId = 1,
            AssetSubclassId = 1,
            Isin = "GB0002634946"
        });
        await _context.SaveChangesAsync();

        var dto = ValidDto();
        dto.Isin = "GB0002634946";

        var validationContext = new ValidationContext<InstrumentWriteDto>(dto);
        validationContext.RootContextData[ValidationExtensions.EntityIdKey] = 10;

        var result = await _validator.ValidateAsync(validationContext);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Isin" && e.ErrorMessage == "An instrument with this ISIN already exists");
    }

    // --- DataFeeds ---

    [Fact]
    public async Task DataFeeds_Null_Passes()
    {
        var dto = ValidDto();
        dto.DataFeeds = null;
        var result = await _validator.ValidateAsync(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DataFeeds_ValidEnabledFeed_Passes()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = true, Ticker = "TEST.L" }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task DataFeeds_EnabledWithoutTicker_Fails()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = true, Ticker = null }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Ticker is required when feed is enabled");
    }

    [Fact]
    public async Task DataFeeds_EnabledWithInvalidDataFeedId_Fails()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 999, Enabled = true, Ticker = "TEST.L" }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Invalid DataFeedId");
    }

    [Fact]
    public async Task DataFeeds_TickerTooLong_Fails()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = true, Ticker = new string('A', 21) }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Ticker cannot exceed 20 characters");
    }

    [Fact]
    public async Task DataFeeds_DuplicateEnabledFeeds_Fails()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = true, Ticker = "TEST.L" },
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = true, Ticker = "TEST2.L" }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Duplicate data feeds are not allowed");
    }

    [Fact]
    public async Task DataFeeds_DuplicateDisabledFeeds_Passes()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = false },
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = false }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Duplicate data feeds are not allowed");
    }

    [Fact]
    public async Task DataFeeds_DisabledFeedWithoutTicker_Passes()
    {
        var dto = ValidDto();
        dto.DataFeeds =
        [
            new InstrumentDataFeedWriteDto { DataFeedId = 1, Enabled = false, Ticker = null }
        ];
        var result = await _validator.ValidateAsync(dto);
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Ticker is required when feed is enabled");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
