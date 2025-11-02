namespace Hoard.Core.Messages;

/// <summary>
/// Refresh quotes for instruments.
/// This will be scheduled to run frequently during trading hours.
/// 
/// The handler of this coarse-grained command will determine which instruments should
/// have their quotes updated on each invocation, and in what batch sizes, and use this to
/// raise multiple <see cref="RefreshQuotesBatchCommand"/> commands.
/// </summary>
public record RefreshQuotesCommand;