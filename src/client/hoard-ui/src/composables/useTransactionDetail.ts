import { ref } from 'vue'
import type { TransactionDetailDto } from '@/api/dtos/Transactions/TransactionDetailDto'
import type { TransactionWriteDto } from '@/api/dtos/Transactions/TransactionWriteDto'
import {
  getTransactionDetail,
  createTransaction,
  updateTransaction,
  deleteTransaction,
} from '@/api/transactionsApi'

export function useTransactionDetail() {
  const transaction = ref<TransactionDetailDto | null>(null)
  const isLoading = ref(false)
  const isSaving = ref(false)
  const isDeleting = ref(false)
  const error = ref<string | null>(null)

  async function fetchTransaction(id: number): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      transaction.value = await getTransactionDetail(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load transaction'
      console.error('Error fetching transaction:', err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(data: TransactionWriteDto): Promise<number | null> {
    isSaving.value = true
    error.value = null
    try {
      const id = await createTransaction(data)
      return id
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create transaction'
      console.error('Error creating transaction:', err)
      return null
    } finally {
      isSaving.value = false
    }
  }

  async function update(id: number, data: TransactionWriteDto): Promise<boolean> {
    isSaving.value = true
    error.value = null
    try {
      await updateTransaction(id, data)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update transaction'
      console.error('Error updating transaction:', err)
      return false
    } finally {
      isSaving.value = false
    }
  }

  async function remove(id: number): Promise<boolean> {
    isDeleting.value = true
    error.value = null
    try {
      await deleteTransaction(id)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to delete transaction'
      console.error('Error deleting transaction:', err)
      return false
    } finally {
      isDeleting.value = false
    }
  }

  return {
    transaction,
    isLoading,
    isSaving,
    isDeleting,
    error,
    fetchTransaction,
    create,
    update,
    remove,
  }
}
