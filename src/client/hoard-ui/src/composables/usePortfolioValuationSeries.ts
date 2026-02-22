import { ref } from 'vue'
import { getPortfolioValuationSeries } from '@/api/portfoliosApi'
import type { DisaggregateBy } from '@/api/portfoliosApi'
import type { PortfolioValuationSeriesGroupDto } from '@/api/dtos/Portfolios/PortfolioValuationSeriesDto'

export function usePortfolioValuationSeries() {
  const series = ref<PortfolioValuationSeriesGroupDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchSeries(
    portfolioId: number,
    from?: string,
    to?: string,
    disaggregateBy?: DisaggregateBy
  ) {
    isLoading.value = true
    error.value = null
    try {
      const result = await getPortfolioValuationSeries(portfolioId, {
        from,
        to,
        disaggregateBy,
      })
      series.value = result.series
    } catch (e) {
      error.value = 'Failed to load portfolio valuation series'
      console.error('Failed to load portfolio valuation series:', e)
      series.value = []
    } finally {
      isLoading.value = false
    }
  }

  return {
    series,
    isLoading,
    error,
    fetchSeries,
  }
}
