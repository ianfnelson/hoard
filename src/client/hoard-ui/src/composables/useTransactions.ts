import { ref } from 'vue'
import { getTransactions, type GetTransactionsParams } from '@/api/transactionsApi.ts'
import type { TransactionSummaryDto } from '@/api/dtos/Transactions/TransactionSummaryDto'

export function useTransactions() {
  const items = ref<TransactionSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchTransactions(params?: GetTransactionsParams) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getTransactions(params)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      error.value = 'Failed to load transactions'
      console.error('Failed to load transactions:', e)
      items.value = []
      totalCount.value = 0
    } finally {
      isLoading.value = false
    }
  }

  return {
    items,
    totalCount,
    isLoading,
    error,
    fetchTransactions,
  }
}
