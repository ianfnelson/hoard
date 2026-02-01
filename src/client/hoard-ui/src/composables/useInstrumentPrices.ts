import { ref } from 'vue'
import { getPrices, type GetPricesParams } from '@/api/instrumentsApi.ts'
import type { PriceSummaryDto } from '@/api/dtos/Instruments/PriceSummaryDto'

export function useInstrumentPrices() {
  const items = ref<PriceSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPrices(instrumentId: number, params?: GetPricesParams) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getPrices(instrumentId, params)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      error.value = 'Failed to load prices'
      console.error('Failed to load prices:', e)
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
    fetchPrices,
  }
}
