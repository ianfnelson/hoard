import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { useInstrumentPrices } from '../useInstrumentPrices'
import * as instrumentsApi from '@/api/instrumentsApi'
import type { PriceSummaryDto } from '@/api/dtos/Instruments/PriceSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

vi.mock('@/api/instrumentsApi')

describe('useInstrumentPrices', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, totalCount, isLoading, error } = useInstrumentPrices()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch prices successfully', async () => {
    const mockResponse: PagedResult<PriceSummaryDto> = {
      items: [{ id: 1, close: 150.0 } as PriceSummaryDto],
      totalCount: 1,
    }
    vi.mocked(instrumentsApi.getPrices).mockResolvedValue(mockResponse)

    const { items, totalCount, fetchPrices } = useInstrumentPrices()
    await fetchPrices(42, { pageSize: 10 })

    expect(items.value).toEqual(mockResponse.items)
    expect(totalCount.value).toBe(1)
    expect(instrumentsApi.getPrices).toHaveBeenCalledWith(
      42,
      { pageSize: 10 },
      expect.any(AbortSignal)
    )
  })

  it('should silently ignore cancelled requests', async () => {
    vi.mocked(instrumentsApi.getPrices).mockRejectedValue(new axios.CanceledError())
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { error, isLoading, fetchPrices } = useInstrumentPrices()
    await fetchPrices(1)

    expect(error.value).toBeNull()
    expect(isLoading.value).toBe(false)
  })

  it('should handle error', async () => {
    vi.mocked(instrumentsApi.getPrices).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, error, fetchPrices } = useInstrumentPrices()
    await fetchPrices(1)

    expect(items.value).toEqual([])
    expect(error.value).toBe('Failed to load prices')
  })
})
