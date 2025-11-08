namespace Hoard.Messages.Transactions;

public record TransactionDeletedEvent(int TransactionId, DateOnly TransactionDate);