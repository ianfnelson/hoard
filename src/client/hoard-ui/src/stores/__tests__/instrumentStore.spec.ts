import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useInstrumentStore } from '../instrumentStore'
import * as instrumentsApi from '@/api/instrumentsApi'

vi.mock('@/api/instrumentsApi')

const mockSubscribe = vi.fn()
const mockUnsubscribe = vi.fn()

vi.mock('@/composables/useSignalRSubscription', () => ({
  useSignalRSubscription: () => ({
    subscribe: mockSubscribe,
    unsubscribe: mockUnsubscribe,
  }),
}))

describe('instrumentStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const store = useInstrumentStore()

    expect(store.instrumentId).toBeNull()
    expect(store.instrument).toBeNull()
    expect(store.isLoading).toBe(false)
    expect(store.lastUpdated).toBeNull()
    expect(store.error).toBeNull()
  })

  describe('load', () => {
    it('should subscribe and fetch instrument data', async () => {
      const mockInstrument = { id: 1, name: 'Fundsmith Equity' }
      vi.mocked(instrumentsApi.getInstrumentDetail).mockResolvedValue(mockInstrument as never)

      const store = useInstrumentStore()
      await store.load(1)

      expect(mockSubscribe).toHaveBeenCalledWith(1)
      expect(store.instrumentId).toBe(1)
      expect(store.instrument).toEqual(mockInstrument)
      expect(store.lastUpdated).not.toBeNull()
      expect(store.isLoading).toBe(false)
    })

    it('should unsubscribe and reset when switching instrument', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockResolvedValue({ id: 1 } as never)

      const store = useInstrumentStore()
      await store.load(1)

      vi.mocked(instrumentsApi.getInstrumentDetail).mockResolvedValue({ id: 2 } as never)
      await store.load(2)

      expect(mockUnsubscribe).toHaveBeenCalled()
      expect(mockSubscribe).toHaveBeenCalledWith(2)
      expect(store.instrumentId).toBe(2)
    })

    it('should not unsubscribe when reloading same instrument', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockResolvedValue({ id: 1 } as never)

      const store = useInstrumentStore()
      await store.load(1)

      mockUnsubscribe.mockClear()
      await store.load(1)

      expect(mockUnsubscribe).not.toHaveBeenCalled()
    })

    it('should set error on API failure', async () => {
      vi.mocked(instrumentsApi.getInstrumentDetail).mockRejectedValue(new Error('fail'))

      const store = useInstrumentStore()

      await expect(store.load(1)).rejects.toThrow('fail')

      expect(store.error).toBe('Failed to load instrument data')
      expect(store.isLoading).toBe(false)
    })
  })
})
