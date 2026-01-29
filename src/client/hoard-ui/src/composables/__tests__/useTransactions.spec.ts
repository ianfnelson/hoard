import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useTransactions } from '../useTransactions'
import * as transactionsApi from '@/api/transactionsApi'
import type { TransactionSummaryDto } from '@/api/dtos/TransactionSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

vi.mock('@/api/transactionsApi')

describe('useTransactions', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, totalCount, isLoading, error } = useTransactions()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch transactions successfully', async () => {
    const mockTransactions: TransactionSummaryDto[] = [
      {
        id: 1,
        accountId: 1,
        accountName: 'Test Account',
        contractNoteReference: 'REF001',
        date: '2024-01-15',
        transactionTypeId: 1,
        transactionTypeName: 'Buy',
        value: 1000,
      },
    ]
    const mockResponse: PagedResult<TransactionSummaryDto> = {
      items: mockTransactions,
      totalCount: 1,
    }

    vi.mocked(transactionsApi.getTransactions).mockResolvedValue(mockResponse)

    const { items, totalCount, isLoading, error, fetchTransactions } = useTransactions()

    const fetchPromise = fetchTransactions()
    expect(isLoading.value).toBe(true)

    await fetchPromise

    expect(items.value).toEqual(mockTransactions)
    expect(totalCount.value).toBe(1)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
    expect(transactionsApi.getTransactions).toHaveBeenCalledOnce()
  })

  it('should handle fetch error', async () => {
    vi.mocked(transactionsApi.getTransactions).mockRejectedValue(new Error('Network error'))

    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, totalCount, isLoading, error, fetchTransactions } = useTransactions()

    await fetchTransactions()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBe('Failed to load transactions')
    expect(consoleErrorSpy).toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })

  it('should pass params to API', async () => {
    const mockResponse: PagedResult<TransactionSummaryDto> = {
      items: [],
      totalCount: 0,
    }

    vi.mocked(transactionsApi.getTransactions).mockResolvedValue(mockResponse)

    const { fetchTransactions } = useTransactions()
    await fetchTransactions({ accountId: 5, pageSize: 10 })

    expect(transactionsApi.getTransactions).toHaveBeenCalledWith({ accountId: 5, pageSize: 10 })
  })
})
