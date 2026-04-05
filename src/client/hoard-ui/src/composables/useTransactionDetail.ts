import { ref, onUnmounted } from 'vue'
import axios from 'axios'
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
  let abortController: AbortController | null = null

  async function fetchTransaction(id: number): Promise<void> {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      transaction.value = await getTransactionDetail(id, controller.signal)
    } catch (err) {
      if (axios.isCancel(err)) return
      error.value = err instanceof Error ? err.message : 'Failed to load transaction'
      console.error('Error fetching transaction:', err)
    } finally {
      if (abortController === controller) {
        isLoading.value = false
      }
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

  onUnmounted(() => {
    abortController?.abort()
  })

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
