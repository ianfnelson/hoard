import { ref } from 'vue'
import { getValuationSeries } from '@/api/instrumentsApi'
import type { ValuationSeriesAccountDto } from '@/api/dtos/Instruments/ValuationSeriesDto'

export function useHoldingValuationSeries() {
  const series = ref<ValuationSeriesAccountDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchHoldingValuationSeries(instrumentId: number) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getValuationSeries(instrumentId)
      series.value = result.series
    } catch (e) {
      error.value = 'Failed to load valuation series'
      console.error('Failed to load valuation series:', e)
      series.value = []
    } finally {
      isLoading.value = false
    }
  }

  return {
    series,
    isLoading,
    error,
    fetchHoldingValuationSeries,
  }
}
