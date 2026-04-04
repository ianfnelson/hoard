import { describe, it, expect, vi, beforeEach } from 'vitest'
import { usePriceSeries } from '../usePriceSeries'
import * as instrumentsApi from '@/api/instrumentsApi'

vi.mock('@/api/instrumentsApi')

describe('usePriceSeries', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { candles, isLoading, error } = usePriceSeries()

    expect(candles.value).toEqual([])
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch price series successfully', async () => {
    const mockCandles = [{ date: '2026-01-01', open: 100, high: 105, low: 99, close: 103 }]
    vi.mocked(instrumentsApi.getPriceSeries).mockResolvedValue({
      candles: mockCandles,
    } as never)

    const { candles, fetchPriceSeries } = usePriceSeries()
    await fetchPriceSeries(42, '2025-01-01', '2026-01-01')

    expect(candles.value).toEqual(mockCandles)
    expect(instrumentsApi.getPriceSeries).toHaveBeenCalledWith(42, '2025-01-01', '2026-01-01')
  })

  it('should handle error', async () => {
    vi.mocked(instrumentsApi.getPriceSeries).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { candles, error, fetchPriceSeries } = usePriceSeries()
    await fetchPriceSeries(1, '2025-01-01', '2026-01-01')

    expect(candles.value).toEqual([])
    expect(error.value).toBe('Failed to load price series')
  })
})
