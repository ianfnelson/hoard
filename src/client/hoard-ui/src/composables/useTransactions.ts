import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getTransactions, type GetTransactionsParams } from '@/api/transactionsApi'
import type { TransactionSummaryDto } from '@/api/dtos/Transactions/TransactionSummaryDto'

export function useTransactions() {
  const items = ref<TransactionSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchTransactions(params?: GetTransactionsParams) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getTransactions(params, controller.signal)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load transactions'
      console.error('Failed to load transactions:', e)
      items.value = []
      totalCount.value = 0
    } finally {
      if (abortController === controller) {
        isLoading.value = false
      }
    }
  }

  onUnmounted(() => {
    abortController?.abort()
  })

  return {
    items,
    totalCount,
    isLoading,
    error,
    fetchTransactions,
  }
}
