using Hoard.Core.Application.Portfolios;
using Hoard.Core.Data;
using Hoard.Core.Domain.Entities;
using Hoard.Test.Shared;

namespace Hoard.Test.Unit.Core.Application.Portfolios;

public class SetTargetAllocationsRequestValidatorTests : IDisposable
{
    private readonly HoardContext _context;
    private readonly SetTargetAllocationsRequestValidator _validator;

    public SetTargetAllocationsRequestValidatorTests()
    {
        _context = TestContextFactory.Create();
        _context.AssetClasses.Add(TestData.AssetClass(1, "Equity"));
        _context.AssetSubclasses.AddRange(
            TestData.AssetSubclass(10, "Stocks", 1),
            TestData.AssetSubclass(20, "Bonds", 1),
            TestData.AssetSubclass(AssetSubclass.Cash, "Cash", 1));
        _context.SaveChanges();
        _validator = new SetTargetAllocationsRequestValidator(_context);
    }

    private static SetTargetAllocationsRequest Request(params (int subclassId, decimal target)[] items) =>
        new()
        {
            Allocations = items
                .Select(i => new TargetAllocationItemDto { AssetSubclassId = i.subclassId, Target = i.target })
                .ToList()
        };

    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        var result = await _validator.ValidateAsync(Request((10, 60m), (20, 30m)));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SumExactly100_Passes()
    {
        var result = await _validator.ValidateAsync(Request((10, 60m), (20, 40m)));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task EmptyAllocations_Passes()
    {
        var result = await _validator.ValidateAsync(Request());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task NegativeTarget_Fails()
    {
        var result = await _validator.ValidateAsync(Request((10, -1m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("between 0 and 100"));
    }

    [Fact]
    public async Task TargetOver100_Fails()
    {
        var result = await _validator.ValidateAsync(Request((10, 101m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("between 0 and 100"));
    }

    [Fact]
    public async Task SumOver100_Fails()
    {
        var result = await _validator.ValidateAsync(Request((10, 60m), (20, 50m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("exceed 100"));
    }

    [Fact]
    public async Task DuplicateSubclass_Fails()
    {
        var result = await _validator.ValidateAsync(Request((10, 30m), (10, 20m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Duplicate"));
    }

    [Fact]
    public async Task UnknownSubclass_Fails()
    {
        var result = await _validator.ValidateAsync(Request((999, 50m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("do not exist"));
    }

    [Fact]
    public async Task CashInPayload_Fails()
    {
        var result = await _validator.ValidateAsync(Request((10, 50m), (AssetSubclass.Cash, 50m)));
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Cash"));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
