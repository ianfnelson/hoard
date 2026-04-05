import { ref, onUnmounted } from 'vue'
import axios from 'axios'
import { getPortfolioValuationSeries } from '@/api/portfoliosApi'
import type { DisaggregateBy } from '@/api/portfoliosApi'
import type { PortfolioValuationSeriesGroupDto } from '@/api/dtos/Portfolios/PortfolioValuationSeriesDto'

export function usePortfolioValuationSeries() {
  const series = ref<PortfolioValuationSeriesGroupDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  let abortController: AbortController | null = null

  async function fetchSeries(
    portfolioId: number,
    from?: string,
    to?: string,
    disaggregateBy?: DisaggregateBy
  ) {
    abortController?.abort()
    const controller = new AbortController()
    abortController = controller

    isLoading.value = true
    error.value = null
    try {
      const result = await getPortfolioValuationSeries(
        portfolioId,
        {
          from,
          to,
          disaggregateBy,
        },
        controller.signal
      )
      series.value = result.series
    } catch (e) {
      if (axios.isCancel(e)) return
      error.value = 'Failed to load portfolio valuation series'
      console.error('Failed to load portfolio valuation series:', e)
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
    fetchSeries,
  }
}
