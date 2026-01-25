export interface TransactionDto {
  id: number;
  accountId: number;
  date: string; // ISO date: YYYY-MM-DD
  instrumentId?: number;
  transactionTypeId: number;
  units?: number;
  value: number;
  notes?: string;
}
