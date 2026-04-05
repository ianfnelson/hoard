import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { useInstruments } from '../useInstruments'
import * as instrumentsApi from '@/api/instrumentsApi'
import type { InstrumentSummaryDto } from '@/api/dtos/Instruments/InstrumentSummaryDto'
import type { PagedResult } from '@/api/dtos/PagedResult'

vi.mock('@/api/instrumentsApi')

describe('useInstruments', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, totalCount, isLoading, error } = useInstruments()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch instruments successfully', async () => {
    const mockResponse: PagedResult<InstrumentSummaryDto> = {
      items: [{ id: 1, name: 'Fundsmith' } as InstrumentSummaryDto],
      totalCount: 1,
    }
    vi.mocked(instrumentsApi.getInstruments).mockResolvedValue(mockResponse)

    const { items, totalCount, fetchInstruments } = useInstruments()
    await fetchInstruments({ search: 'fund' })

    expect(items.value).toEqual(mockResponse.items)
    expect(totalCount.value).toBe(1)
    expect(instrumentsApi.getInstruments).toHaveBeenCalledWith(
      { search: 'fund' },
      expect.any(AbortSignal)
    )
  })

  it('should silently ignore cancelled requests', async () => {
    vi.mocked(instrumentsApi.getInstruments).mockRejectedValue(new axios.CanceledError())
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { error, isLoading, fetchInstruments } = useInstruments()
    await fetchInstruments()

    expect(error.value).toBeNull()
    expect(isLoading.value).toBe(false)
  })

  it('should handle error', async () => {
    vi.mocked(instrumentsApi.getInstruments).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, totalCount, error, fetchInstruments } = useInstruments()
    await fetchInstruments()

    expect(items.value).toEqual([])
    expect(totalCount.value).toBe(0)
    expect(error.value).toBe('Failed to load instruments')
  })
})
