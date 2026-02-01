import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getPortfolioList } from '@/api/portfoliosApi'
import type { PortfolioSummaryDto } from '@/api/dtos/Portfolios/PortfolioSummaryDto'
import { getAccounts } from '@/api/accountsApi'
import type { AccountSummaryDto } from '@/api/dtos/Accounts/AccountSummaryDto'

export const useNavigationStore = defineStore('navigation', () => {
  const portfolios = ref<PortfolioSummaryDto[]>([])
  const accounts = ref<AccountSummaryDto[]>([])
  const isLoading = ref(false)
  const lastUpdated = ref<Date | null>(null)
  const accountsLoaded = ref(false)
  const error = ref<string | null>(null)

  async function loadPortfolios() {
    isLoading.value = true
    error.value = null
    try {
      portfolios.value = await getPortfolioList()
      lastUpdated.value = new Date()
    } catch (e) {
      error.value = 'Failed to load portfolios'
      console.error('Failed to load portfolios:', e)
      portfolios.value = []
    } finally {
      isLoading.value = false
    }
  }

  async function loadAccounts() {
    if (accountsLoaded.value) return

    isLoading.value = true
    error.value = null
    try {
      accounts.value = await getAccounts({ isActive: true })
      accountsLoaded.value = true
    } catch (e) {
      error.value = 'Failed to load accounts'
      console.error('Failed to load accounts:', e)
      accounts.value = []
    } finally {
      isLoading.value = false
    }
  }

  async function refresh() {
    lastUpdated.value = null
    accountsLoaded.value = false
    await Promise.all([loadPortfolios(), loadAccounts()])
  }

  return {
    portfolios,
    accounts,
    isLoading,
    lastUpdated,
    error,
    loadPortfolios,
    loadAccounts,
    refresh,
  }
})
