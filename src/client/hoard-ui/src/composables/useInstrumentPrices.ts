import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getPrices, type GetPricesParams } from '@/api/instrumentsApi'
import type { PriceSummaryDto } from '@/api/dtos/Instruments/PriceSummaryDto'

export function useInstrumentPrices() {
  const items = ref<PriceSummaryDto[]>([])
  const totalCount = ref(0)
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchPrices(instrumentId: number, params?: GetPricesParams) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getPrices(instrumentId, params, controller.signal)
      items.value = result.items
      totalCount.value = result.totalCount
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load prices'
      console.error('Failed to load prices:', e)
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
    fetchPrices,
  }
}
