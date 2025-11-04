namespace Hoard.Core.Messages.Valuations;

/// <summary>
/// Calculate valuations of accounts at a given date
/// </summary>
/// <param name="AsOfDate">Date on which to refresh valuation</param>
public record CalculateAccountValuationsCommand(DateOnly AsOfDate);