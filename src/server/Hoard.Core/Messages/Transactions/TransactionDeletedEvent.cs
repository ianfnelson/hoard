namespace Hoard.Core.Messages.Transactions;

public record TransactionDeletedEvent(int TransactionId, DateOnly TransactionDate);