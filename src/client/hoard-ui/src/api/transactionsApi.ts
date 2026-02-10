import { hoardApi } from './hoardApi'
import type { TransactionSummaryDto } from './dtos/Transactions/TransactionSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'
import type { TransactionDetailDto } from './dtos/Transactions/TransactionDetailDto'
import type { TransactionWriteDto } from './dtos/Transactions/TransactionWriteDto'

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

export async function createTransaction(data: TransactionWriteDto): Promise<number> {
  const response = await hoardApi.post('/transactions', data)
  return response.data
}

export async function updateTransaction(id: number, data: TransactionWriteDto): Promise<void> {
  await hoardApi.put(`/transactions/${id}`, data)
}

export async function deleteTransaction(id: number): Promise<void> {
  await hoardApi.delete(`/transactions/${id}`)
}

export interface ContractNoteUploadResponse {
  reference: string
  blobUri: string
}

export async function uploadContractNote(
  transactionId: number,
  file: File
): Promise<ContractNoteUploadResponse> {
  const formData = new FormData()
  formData.append('file', file)

  const response = await hoardApi.post(`/transactions/${transactionId}/contractnote`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
  return response.data
}

export async function downloadContractNote(transactionId: number): Promise<Blob> {
  const response = await hoardApi.get(`/transactions/${transactionId}/contractnote`, {
    responseType: 'blob',
  })
  return response.data
}

export async function deleteContractNote(transactionId: number): Promise<void> {
  await hoardApi.delete(`/transactions/${transactionId}/contractnote`)
}
