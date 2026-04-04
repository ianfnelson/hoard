import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useHoldingValuationSeries } from '../useHoldingValuationSeries'
import * as instrumentsApi from '@/api/instrumentsApi'

vi.mock('@/api/instrumentsApi')

describe('useHoldingValuationSeries', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { series, isLoading, error } = useHoldingValuationSeries()

    expect(series.value).toEqual([])
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch valuation series successfully', async () => {
    const mockSeries = [{ accountName: 'ISA', dataPoints: [] }]
    vi.mocked(instrumentsApi.getValuationSeries).mockResolvedValue({
      series: mockSeries,
    } as never)

    const { series, fetchHoldingValuationSeries } = useHoldingValuationSeries()
    await fetchHoldingValuationSeries(42)

    expect(series.value).toEqual(mockSeries)
    expect(instrumentsApi.getValuationSeries).toHaveBeenCalledWith(42)
  })

  it('should handle error', async () => {
    vi.mocked(instrumentsApi.getValuationSeries).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { series, error, fetchHoldingValuationSeries } = useHoldingValuationSeries()
    await fetchHoldingValuationSeries(1)

    expect(series.value).toEqual([])
    expect(error.value).toBe('Failed to load valuation series')
  })
})
