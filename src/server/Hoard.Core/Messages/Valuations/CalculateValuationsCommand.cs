namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Trigger the calculation of all valuations, for the specified date.
/// </summary>
public record CalculateValuationsCommand
{
    /// <summary>
    /// Date for which valuations to be calculated.
    /// If not specified, defaults to today.
    /// </summary>
    public DateOnly? AsOfDate { get; init; }
}