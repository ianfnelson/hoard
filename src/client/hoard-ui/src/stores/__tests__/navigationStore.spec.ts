import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useNavigationStore } from '../navigationStore'
import * as portfoliosApi from '@/api/portfoliosApi'
import * as accountsApi from '@/api/accountsApi'
import type { PortfolioSummaryDto } from '@/api/dtos/Portfolios/PortfolioSummaryDto'
import type { AccountSummaryDto } from '@/api/dtos/Accounts/AccountSummaryDto'

vi.mock('@/api/portfoliosApi')
vi.mock('@/api/accountsApi')

describe('navigationStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('should have correct initial state', () => {
    const store = useNavigationStore()

    expect(store.portfolios).toEqual([])
    expect(store.accounts).toEqual([])
    expect(store.isLoading).toBe(false)
    expect(store.lastUpdated).toBeNull()
    expect(store.error).toBeNull()
  })

  describe('loadPortfolios', () => {
    it('should load portfolios successfully', async () => {
      const mockPortfolios: PortfolioSummaryDto[] = [
        { id: 1, name: 'ISA', isActive: true, createdUtc: '2026-01-01T00:00:00Z' },
        { id: 2, name: 'SIPP', isActive: true, createdUtc: '2026-01-01T00:00:00Z' },
      ]
      vi.mocked(portfoliosApi.getPortfolioList).mockResolvedValue(mockPortfolios)

      const store = useNavigationStore()
      await store.loadPortfolios()

      expect(store.portfolios).toEqual(mockPortfolios)
      expect(store.lastUpdated).not.toBeNull()
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('should not reload if already loaded', async () => {
      vi.mocked(portfoliosApi.getPortfolioList).mockResolvedValue([])

      const store = useNavigationStore()
      await store.loadPortfolios()
      await store.loadPortfolios()

      expect(portfoliosApi.getPortfolioList).toHaveBeenCalledOnce()
    })

    it('should handle error', async () => {
      vi.mocked(portfoliosApi.getPortfolioList).mockRejectedValue(new Error('Network error'))
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      const store = useNavigationStore()
      await store.loadPortfolios()

      expect(store.portfolios).toEqual([])
      expect(store.error).toBe('Failed to load portfolios')
      expect(store.isLoading).toBe(false)
      expect(consoleErrorSpy).toHaveBeenCalled()

      consoleErrorSpy.mockRestore()
    })
  })

  describe('loadAccounts', () => {
    it('should load accounts successfully', async () => {
      const mockAccounts: AccountSummaryDto[] = [
        { id: 1, name: 'Trading Account', isActive: true, createdUtc: '2026-01-01T00:00:00Z' },
      ]
      vi.mocked(accountsApi.getAccounts).mockResolvedValue(mockAccounts)

      const store = useNavigationStore()
      await store.loadAccounts()

      expect(store.accounts).toEqual(mockAccounts)
      expect(accountsApi.getAccounts).toHaveBeenCalledWith({ isActive: true })
    })

    it('should not reload if already loaded', async () => {
      vi.mocked(accountsApi.getAccounts).mockResolvedValue([])

      const store = useNavigationStore()
      await store.loadAccounts()
      await store.loadAccounts()

      expect(accountsApi.getAccounts).toHaveBeenCalledOnce()
    })

    it('should handle error', async () => {
      vi.mocked(accountsApi.getAccounts).mockRejectedValue(new Error('fail'))
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

      const store = useNavigationStore()
      await store.loadAccounts()

      expect(store.accounts).toEqual([])
      expect(store.error).toBe('Failed to load accounts')

      consoleErrorSpy.mockRestore()
    })
  })

  describe('refresh', () => {
    it('should clear caches and reload all data', async () => {
      vi.mocked(portfoliosApi.getPortfolioList).mockResolvedValue([])
      vi.mocked(accountsApi.getAccounts).mockResolvedValue([])

      const store = useNavigationStore()

      // First load to set cache flags
      await store.loadPortfolios()
      await store.loadAccounts()
      expect(portfoliosApi.getPortfolioList).toHaveBeenCalledOnce()
      expect(accountsApi.getAccounts).toHaveBeenCalledOnce()

      // Refresh should reload even though caches were set
      await store.refresh()
      expect(portfoliosApi.getPortfolioList).toHaveBeenCalledTimes(2)
      expect(accountsApi.getAccounts).toHaveBeenCalledTimes(2)
    })
  })
})
