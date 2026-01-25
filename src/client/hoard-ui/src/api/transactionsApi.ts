import { hoardApi } from "./hoardApi";
import type {TransactionSummaryDto} from "./dtos/TransactionSummaryDto.ts";
import type { PagedResult } from "@/api/dtos/PagedResult.ts";
import type {TransactionDetailDto} from "@/api/dtos/TransactionDetailDto.ts";

export interface GetTransactionsParams {
  accountId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export async function getTransactions(
  params?: GetTransactionsParams
): Promise<PagedResult<TransactionSummaryDto>> {
  const response = await hoardApi.get("/transactions", { params });
  return response.data;
}

export async function getTransactionDetail(
  transactionId: number
): Promise<TransactionDetailDto> {
  const response = await hoardApi.get(
    `/transactions/${transactionId}`
  );
  return response.data;
}
