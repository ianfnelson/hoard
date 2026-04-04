import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useTransactionDetail } from '../useTransactionDetail'
import * as transactionsApi from '@/api/transactionsApi'
import type { TransactionDetailDto } from '@/api/dtos/Transactions/TransactionDetailDto'
import type { TransactionWriteDto } from '@/api/dtos/Transactions/TransactionWriteDto'

vi.mock('@/api/transactionsApi')

describe('useTransactionDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { transaction, isLoading, isSaving, isDeleting, error } = useTransactionDetail()

    expect(transaction.value).toBeNull()
    expect(isLoading.value).toBe(false)
    expect(isSaving.value).toBe(false)
    expect(isDeleting.value).toBe(false)
    expect(error.value).toBeNull()
  })

  describe('fetchTransaction', () => {
    it('should fetch successfully', async () => {
      const mockTransaction = { id: 1, date: '2026-01-01' } as TransactionDetailDto
      vi.mocked(transactionsApi.getTransactionDetail).mockResolvedValue(mockTransaction)

      const { transaction, isLoading, error, fetchTransaction } = useTransactionDetail()

      const promise = fetchTransaction(1)
      expect(isLoading.value).toBe(true)

      await promise

      expect(transaction.value).toEqual(mockTransaction)
      expect(isLoading.value).toBe(false)
      expect(error.value).toBeNull()
    })

    it('should use Error.message on failure', async () => {
      vi.mocked(transactionsApi.getTransactionDetail).mockRejectedValue(new Error('Not found'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, fetchTransaction } = useTransactionDetail()
      await fetchTransaction(99)

      expect(error.value).toBe('Not found')
    })

    it('should use fallback message for non-Error throws', async () => {
      vi.mocked(transactionsApi.getTransactionDetail).mockRejectedValue('string error')
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, fetchTransaction } = useTransactionDetail()
      await fetchTransaction(1)

      expect(error.value).toBe('Failed to load transaction')
    })
  })

  describe('create', () => {
    it('should create and return id', async () => {
      vi.mocked(transactionsApi.createTransaction).mockResolvedValue(10)

      const { isSaving, create } = useTransactionDetail()
      const data = { date: '2026-01-01' } as TransactionWriteDto

      const promise = create(data)
      expect(isSaving.value).toBe(true)

      const id = await promise
      expect(id).toBe(10)
      expect(isSaving.value).toBe(false)
    })

    it('should return null on error', async () => {
      vi.mocked(transactionsApi.createTransaction).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { error, create } = useTransactionDetail()
      const id = await create({} as TransactionWriteDto)

      expect(id).toBeNull()
      expect(error.value).toBe('fail')
    })
  })

  describe('update', () => {
    it('should update and return true', async () => {
      vi.mocked(transactionsApi.updateTransaction).mockResolvedValue(undefined)

      const { update } = useTransactionDetail()
      const result = await update(1, {} as TransactionWriteDto)

      expect(result).toBe(true)
    })

    it('should return false on error', async () => {
      vi.mocked(transactionsApi.updateTransaction).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { update } = useTransactionDetail()
      const result = await update(1, {} as TransactionWriteDto)

      expect(result).toBe(false)
    })
  })

  describe('remove', () => {
    it('should delete and return true', async () => {
      vi.mocked(transactionsApi.deleteTransaction).mockResolvedValue(undefined)

      const { isDeleting, remove } = useTransactionDetail()

      const promise = remove(1)
      expect(isDeleting.value).toBe(true)

      const result = await promise
      expect(result).toBe(true)
      expect(isDeleting.value).toBe(false)
    })

    it('should return false on error', async () => {
      vi.mocked(transactionsApi.deleteTransaction).mockRejectedValue(new Error('fail'))
      vi.spyOn(console, 'error').mockImplementation(() => {})

      const { remove } = useTransactionDetail()
      const result = await remove(1)

      expect(result).toBe(false)
    })
  })
})
