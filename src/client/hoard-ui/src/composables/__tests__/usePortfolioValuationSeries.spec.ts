import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { usePortfolioValuationSeries } from '../usePortfolioValuationSeries'
import * as portfoliosApi from '@/api/portfoliosApi'

vi.mock('@/api/portfoliosApi')

describe('usePortfolioValuationSeries', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { series, isLoading, error } = usePortfolioValuationSeries()

    expect(series.value).toEqual([])
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch series successfully', async () => {
    const mockSeries = [{ name: 'Total', dataPoints: [] }]
    vi.mocked(portfoliosApi.getPortfolioValuationSeries).mockResolvedValue({
      series: mockSeries,
    } as never)

    const { series, fetchSeries } = usePortfolioValuationSeries()
    await fetchSeries(1, '2025-01-01', '2026-01-01', 'accounts')

    expect(series.value).toEqual(mockSeries)
    expect(portfoliosApi.getPortfolioValuationSeries).toHaveBeenCalledWith(
      1,
      {
        from: '2025-01-01',
        to: '2026-01-01',
        disaggregateBy: 'accounts',
      },
      expect.any(AbortSignal)
    )
  })

  it('should silently ignore cancelled requests', async () => {
    vi.mocked(portfoliosApi.getPortfolioValuationSeries).mockRejectedValue(
      new axios.CanceledError()
    )
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { error, isLoading, fetchSeries } = usePortfolioValuationSeries()
    await fetchSeries(1)

    expect(error.value).toBeNull()
    expect(isLoading.value).toBe(false)
  })

  it('should handle error', async () => {
    vi.mocked(portfoliosApi.getPortfolioValuationSeries).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { series, error, fetchSeries } = usePortfolioValuationSeries()
    await fetchSeries(1)

    expect(series.value).toEqual([])
    expect(error.value).toBe('Failed to load portfolio valuation series')
  })
})
