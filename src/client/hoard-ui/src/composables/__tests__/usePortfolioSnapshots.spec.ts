import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { usePortfolioSnapshots } from '../usePortfolioSnapshots'
import * as portfoliosApi from '@/api/portfoliosApi'

vi.mock('@/api/portfoliosApi')

describe('usePortfolioSnapshots', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const { items, isLoading, error } = usePortfolioSnapshots()

    expect(items.value).toEqual([])
    expect(isLoading.value).toBe(false)
    expect(error.value).toBeNull()
  })

  it('should fetch snapshots successfully', async () => {
    const mockSnapshots = [{ year: 2025, value: 50000 }]
    vi.mocked(portfoliosApi.getPortfolioSnapshots).mockResolvedValue({
      snapshots: mockSnapshots,
    } as never)

    const { items, fetchSnapshots } = usePortfolioSnapshots()
    await fetchSnapshots(1)

    expect(items.value).toEqual(mockSnapshots)
    expect(portfoliosApi.getPortfolioSnapshots).toHaveBeenCalledWith(1, expect.any(AbortSignal))
  })

  it('should silently ignore cancelled requests', async () => {
    vi.mocked(portfoliosApi.getPortfolioSnapshots).mockRejectedValue(new axios.CanceledError())
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { error, isLoading, fetchSnapshots } = usePortfolioSnapshots()
    await fetchSnapshots(1)

    expect(error.value).toBeNull()
    expect(isLoading.value).toBe(false)
  })

  it('should handle error', async () => {
    vi.mocked(portfoliosApi.getPortfolioSnapshots).mockRejectedValue(new Error('fail'))
    vi.spyOn(console, 'error').mockImplementation(() => {})

    const { items, error, fetchSnapshots } = usePortfolioSnapshots()
    await fetchSnapshots(1)

    expect(items.value).toEqual([])
    expect(error.value).toBe('Failed to load snapshots')
  })
})
