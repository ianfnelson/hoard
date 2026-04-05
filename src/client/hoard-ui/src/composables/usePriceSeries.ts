import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getPriceSeries } from '@/api/instrumentsApi'
import type { CandleDto } from '@/api/dtos/Instruments/PriceSeriesDto'

export function usePriceSeries() {
  const candles = ref<CandleDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchPriceSeries(instrumentId: number, from: string, to: string) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getPriceSeries(instrumentId, from, to, controller.signal)
      candles.value = result.candles
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load price series'
      console.error('Failed to load price series:', e)
      candles.value = []
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
    candles,
    isLoading,
    error,
    fetchPriceSeries,
  }
}
