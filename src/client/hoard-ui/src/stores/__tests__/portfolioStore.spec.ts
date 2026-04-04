import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePortfolioStore } from '../portfolioStore'
import * as portfoliosApi from '@/api/portfoliosApi'

vi.mock('@/api/portfoliosApi')

const mockSubscribe = vi.fn()
const mockUnsubscribe = vi.fn()

vi.mock('@/composables/useSignalRSubscription', () => ({
  useSignalRSubscription: () => ({
    subscribe: mockSubscribe,
    unsubscribe: mockUnsubscribe,
  }),
}))

describe('portfolioStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const store = usePortfolioStore()

    expect(store.portfolioId).toBeNull()
    expect(store.portfolio).toBeNull()
    expect(store.positions).toBeNull()
    expect(store.exposure).toBeNull()
    expect(store.instrumentTypes).toBeNull()
    expect(store.openPositionsList).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.lastUpdated).toBeNull()
    expect(store.error).toBeNull()
  })

  describe('load', () => {
    it('should subscribe and fetch all portfolio data', async () => {
      const mockPortfolio = { id: 1, name: 'ISA' }
      const mockPositions = {
        positions: [
          { instrumentId: 1, closeDate: null },
          { instrumentId: 2, closeDate: '2025-01-01' },
        ],
      }
      const mockExposure = { totalValue: 10000 }
      const mockInstrumentTypes = { types: [] }

      vi.mocked(portfoliosApi.getPortfolioDetail).mockResolvedValue(mockPortfolio as never)
      vi.mocked(portfoliosApi.getPortfolioPositions).mockResolvedValue(mockPositions as never)
      vi.mocked(portfoliosApi.getPortfolioExposure).mockResolvedValue(mockExposure as never)
      vi.mocked(portfoliosApi.getPortfolioInstrumentTypes).mockResolvedValue(
        mockInstrumentTypes as never
      )

      const store = usePortfolioStore()
      await store.load(1)

      expect(mockSubscribe).toHaveBeenCalledWith(1)
      expect(store.portfolioId).toBe(1)
      expect(store.portfolio).toEqual(mockPortfolio)
      expect(store.positions).toEqual(mockPositions)
      expect(store.exposure).toEqual(mockExposure)
      expect(store.instrumentTypes).toEqual(mockInstrumentTypes)
      expect(store.lastUpdated).not.toBeNull()
      expect(store.isLoading).toBe(false)
    })

    it('should compute openPositionsList from positions without closeDate', async () => {
      const openPosition = { instrumentId: 1, closeDate: null }
      const closedPosition = { instrumentId: 2, closeDate: '2025-01-01' }
      const mockPositions = { positions: [openPosition, closedPosition] }

      vi.mocked(portfoliosApi.getPortfolioDetail).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioPositions).mockResolvedValue(mockPositions as never)
      vi.mocked(portfoliosApi.getPortfolioExposure).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioInstrumentTypes).mockResolvedValue({} as never)

      const store = usePortfolioStore()
      await store.load(1)

      expect(store.openPositionsList).toEqual([openPosition])
    })

    it('should unsubscribe and reset when switching portfolio', async () => {
      vi.mocked(portfoliosApi.getPortfolioDetail).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioPositions).mockResolvedValue({ positions: [] } as never)
      vi.mocked(portfoliosApi.getPortfolioExposure).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioInstrumentTypes).mockResolvedValue({} as never)

      const store = usePortfolioStore()
      await store.load(1)
      await store.load(2)

      expect(mockUnsubscribe).toHaveBeenCalled()
      expect(mockSubscribe).toHaveBeenCalledWith(2)
      expect(store.portfolioId).toBe(2)
    })

    it('should not unsubscribe when reloading same portfolio', async () => {
      vi.mocked(portfoliosApi.getPortfolioDetail).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioPositions).mockResolvedValue({ positions: [] } as never)
      vi.mocked(portfoliosApi.getPortfolioExposure).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioInstrumentTypes).mockResolvedValue({} as never)

      const store = usePortfolioStore()
      await store.load(1)

      mockUnsubscribe.mockClear()
      await store.load(1)

      expect(mockUnsubscribe).not.toHaveBeenCalled()
    })

    it('should set error on API failure', async () => {
      vi.mocked(portfoliosApi.getPortfolioDetail).mockRejectedValue(new Error('fail'))
      vi.mocked(portfoliosApi.getPortfolioPositions).mockResolvedValue({ positions: [] } as never)
      vi.mocked(portfoliosApi.getPortfolioExposure).mockResolvedValue({} as never)
      vi.mocked(portfoliosApi.getPortfolioInstrumentTypes).mockResolvedValue({} as never)

      const store = usePortfolioStore()

      await expect(store.load(1)).rejects.toThrow('fail')

      expect(store.error).toBe('Failed to load portfolio data')
      expect(store.isLoading).toBe(false)
    })
  })
})
