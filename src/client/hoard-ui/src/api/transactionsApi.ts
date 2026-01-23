import { hoardApi } from "./hoardApi";

export interface Transaction {
  id: number;
  accountId: number;
  date: string;
  instrumentId?: number;
  transactionTypeId: number;
  units?: number;
  value: number;
  notes?: string;
}

export interface GetTransactionsParams {
  accountId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export async function getTransactions(
  params?: GetTransactionsParams
): Promise<{ items: Transaction[]; totalCount: number }> {
  const response = await hoardApi.get("/transactions", { params });
  return response.data;
}

export async function createTransaction(
  transaction: Omit<Transaction, "id">
): Promise<Transaction> {
  const response = await hoardApi.post("/transactions", transaction);
  return response.data;
}

export async function updateTransaction(
  id: number,
  transaction: Partial<Transaction>
): Promise<Transaction> {
  const response = await hoardApi.put(`/transactions/${id}`, transaction);
  return response.data;
}

export async function deleteTransaction(id: number): Promise<void> {
  await hoardApi.delete(`/transactions/${id}`);
}
