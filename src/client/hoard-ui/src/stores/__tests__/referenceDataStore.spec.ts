import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useReferenceDataStore } from '../referenceDataStore'
import * as referencesApi from '@/api/referencesApi'
import * as instrumentsApi from '@/api/instrumentsApi'

vi.mock('@/api/referencesApi')
vi.mock('@/api/instrumentsApi')

describe('referenceDataStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const store = useReferenceDataStore()

    expect(store.instrumentTypes).toEqual([])
    expect(store.assetClasses).toEqual([])
    expect(store.transactionTypes).toEqual([])
    expect(store.currencies).toEqual([])
    expect(store.alertTypes).toEqual([])
    expect(store.runLogTypes).toEqual([])
    expect(store.instruments).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.lastUpdated).toBeNull()
    expect(store.error).toBeNull()
  })

  describe('loadInstrumentTypes', () => {
    it('should load instrument types', async () => {
      const mockTypes = [{ id: 1, name: 'Equity', isCash: false, isFxPair: false }]
      vi.mocked(referencesApi.getInstrumentTypes).mockResolvedValue(mockTypes)

      const store = useReferenceDataStore()
      await store.loadInstrumentTypes()

      expect(store.instrumentTypes).toEqual(mockTypes)
      expect(store.lastUpdated).not.toBeNull()
      expect(store.isLoading).toBe(false)
    })

    it('should not reload if already loaded', async () => {
      vi.mocked(referencesApi.getInstrumentTypes).mockResolvedValue([])

      const store = useReferenceDataStore()
      await store.loadInstrumentTypes()
      await store.loadInstrumentTypes()

      expect(referencesApi.getInstrumentTypes).toHaveBeenCalledOnce()
    })

    it('should handle error and clear data', async () => {
      vi.mocked(referencesApi.getInstrumentTypes).mockRejectedValue(new Error('fail'))
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      const store = useReferenceDataStore()
      await store.loadInstrumentTypes()

      expect(store.instrumentTypes).toEqual([])
      expect(store.error).toBe('Failed to load instrument types')

      consoleErrorSpy.mockRestore()
    })
  })

  describe('loadAssetClasses', () => {
    it('should load asset classes', async () => {
      const mockClasses = [{ id: 1, name: 'Equities', subclasses: [] }]
      vi.mocked(referencesApi.getAssetClasses).mockResolvedValue(mockClasses)

      const store = useReferenceDataStore()
      await store.loadAssetClasses()

      expect(store.assetClasses).toEqual(mockClasses)
    })

    it('should not reload if already loaded', async () => {
      vi.mocked(referencesApi.getAssetClasses).mockResolvedValue([])

      const store = useReferenceDataStore()
      await store.loadAssetClasses()
      await store.loadAssetClasses()

      expect(referencesApi.getAssetClasses).toHaveBeenCalledOnce()
    })
  })

  describe('loadCurrencies', () => {
    it('should load currencies', async () => {
      const mockCurrencies = [{ id: 'GBP', name: 'British Pound' }]
      vi.mocked(referencesApi.getCurrencies).mockResolvedValue(mockCurrencies)

      const store = useReferenceDataStore()
      await store.loadCurrencies()

      expect(store.currencies).toEqual(mockCurrencies)
    })
  })

  describe('loadInstruments', () => {
    it('should load instruments lookup', async () => {
      const mockInstruments = [{ id: 1, name: 'Fundsmith Equity', context: 'Fund' }]
      vi.mocked(instrumentsApi.getInstrumentsLookup).mockResolvedValue(mockInstruments)

      const store = useReferenceDataStore()
      await store.loadInstruments()

      expect(store.instruments).toEqual(mockInstruments)
      expect(instrumentsApi.getInstrumentsLookup).toHaveBeenCalledOnce()
    })

    it('should not reload if already loaded', async () => {
      vi.mocked(instrumentsApi.getInstrumentsLookup).mockResolvedValue([])

      const store = useReferenceDataStore()
      await store.loadInstruments()
      await store.loadInstruments()

      expect(instrumentsApi.getInstrumentsLookup).toHaveBeenCalledOnce()
    })
  })

  describe('refresh', () => {
    it('should reset all cache flags and reload everything', async () => {
      vi.mocked(referencesApi.getInstrumentTypes).mockResolvedValue([])
      vi.mocked(referencesApi.getAssetClasses).mockResolvedValue([])
      vi.mocked(referencesApi.getTransactionTypes).mockResolvedValue([])
      vi.mocked(referencesApi.getCurrencies).mockResolvedValue([])
      vi.mocked(referencesApi.getAlertTypes).mockResolvedValue([])
      vi.mocked(referencesApi.getRunLogTypes).mockResolvedValue([])
      vi.mocked(instrumentsApi.getInstrumentsLookup).mockResolvedValue([])

      const store = useReferenceDataStore()

      // Load once to set cache flags
      await store.loadAssetClasses()
      await store.loadInstruments()

      // Refresh should reload all, including cached ones
      await store.refresh()

      expect(referencesApi.getInstrumentTypes).toHaveBeenCalledOnce()
      expect(referencesApi.getAssetClasses).toHaveBeenCalledTimes(2)
      expect(referencesApi.getTransactionTypes).toHaveBeenCalledOnce()
      expect(referencesApi.getCurrencies).toHaveBeenCalledOnce()
      expect(referencesApi.getAlertTypes).toHaveBeenCalledOnce()
      expect(referencesApi.getRunLogTypes).toHaveBeenCalledOnce()
      expect(instrumentsApi.getInstrumentsLookup).toHaveBeenCalledTimes(2)
    })
  })
})
