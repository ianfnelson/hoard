import { hoardApi } from "./hoardApi";
import type { TransactionDto } from "./dtos/TransactionDto";
import type { PagedResult } from "@/api/types/PagedResult";

export interface GetTransactionsParams {
  accountId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export async function getTransactions(
  params?: GetTransactionsParams
): Promise<PagedResult<TransactionDto>> {
  const response = await hoardApi.get("/transactions", { params });
  return response.data;
}

export async function createTransaction(
  transaction: Omit<TransactionDto, "id">
): Promise<TransactionDto> {
  const response = await hoardApi.post("/transactions", transaction);
  return response.data;
}

export async function updateTransaction(
  id: number,
  transaction: Partial<TransactionDto>
): Promise<TransactionDto> {
  const response = await hoardApi.put(`/transactions/${id}`, transaction);
  return response.data;
}

export async function deleteTransaction(id: number): Promise<void> {
  await hoardApi.delete(`/transactions/${id}`);
}
