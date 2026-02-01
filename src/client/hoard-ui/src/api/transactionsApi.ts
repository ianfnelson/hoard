import { hoardApi } from './hoardApi'
import type { TransactionSummaryDto } from './dtos/Transactions/TransactionSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'
import type { TransactionDetailDto } from './dtos/Transactions/TransactionDetailDto'

export interface GetTransactionsParams {
  accountId?: number
  instrumentId?: number
  transactionTypeId?: number
  fromDate?: string
  toDate?: string
  search?: string
  pageNumber?: number
  pageSize?: number
  sortBy?: string
  sortDirection?: string
}

export async function getTransactions(
  params?: GetTransactionsParams
): Promise<PagedResult<TransactionSummaryDto>> {
  const response = await hoardApi.get('/transactions', { params })
  return response.data
}

export async function getTransactionDetail(transactionId: number): Promise<TransactionDetailDto> {
  const response = await hoardApi.get(`/transactions/${transactionId}`)
  return response.data
}
