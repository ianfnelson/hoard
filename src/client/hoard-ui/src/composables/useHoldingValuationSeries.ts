import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getValuationSeries } from '@/api/instrumentsApi'
import type { ValuationSeriesAccountDto } from '@/api/dtos/Instruments/ValuationSeriesDto'

export function useHoldingValuationSeries() {
  const series = ref<ValuationSeriesAccountDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchHoldingValuationSeries(instrumentId: number) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getValuationSeries(instrumentId, controller.signal)
      series.value = result.series
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load valuation series'
      console.error('Failed to load valuation series:', e)
      series.value = []
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
    series,
    isLoading,
    error,
    fetchHoldingValuationSeries,
  }
}
