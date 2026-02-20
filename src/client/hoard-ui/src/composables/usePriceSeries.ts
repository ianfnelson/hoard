import { ref } from 'vue'
import { getPriceSeries } from '@/api/instrumentsApi'
import type { CandleDto } from '@/api/dtos/Instruments/PriceSeriesDto'

export function usePriceSeries() {
  const candles = ref<CandleDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchPriceSeries(instrumentId: number, from: string, to: string) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getPriceSeries(instrumentId, from, to)
      candles.value = result.candles
    } catch (e) {
      error.value = 'Failed to load price series'
      console.error('Failed to load price series:', e)
      candles.value = []
    } finally {
      isLoading.value = false
    }
  }

  return {
    candles,
    isLoading,
    error,
    fetchPriceSeries,
  }
}
