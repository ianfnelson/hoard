import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import {
  getPortfolioDetail,
  getPortfolioPositions,
  getPortfolioExposure,
  getPortfolioInstrumentTypes,
} from '@/api/portfoliosApi'
import { useSignalRSubscription } from '@/composables/useSignalRSubscription'
import type { PortfolioDetailDto } from '@/api/dtos/Portfolios/PortfolioDetailDto'
import type { PortfolioPositionsDto } from '@/api/dtos/Portfolios/PortfolioPositionsDto'
import type { PortfolioExposureDto } from '@/api/dtos/Portfolios/PortfolioExposureDto'
import type { PortfolioInstrumentTypesDto } from '@/api/dtos/Portfolios/PortfolioInstrumentTypesDto'

export const usePortfolioStore = defineStore('portfolio', () => {
  const portfolioId = ref<number | null>(null)

  const portfolio = ref<PortfolioDetailDto | null>(null)
  const positions = ref<PortfolioPositionsDto | null>(null)
  const exposure = ref<PortfolioExposureDto | null>(null)
  const instrumentTypes = ref<PortfolioInstrumentTypesDto | null>(null)

  const openPositionsList = computed(() =>
    (positions.value?.positions ?? []).filter((p) => p.closeDate === null)
  )

  const isLoading = ref(false)
  const lastUpdated = ref<Date | null>(null)
  const error = ref<string | null>(null)

  const { subscribe, unsubscribe } = useSignalRSubscription(
    {
      hubUrl: '/hubs/portfolio',
      eventName: 'PortfolioUpdated',
      subscribeMethod: 'SubscribeToPortfolio',
      unsubscribeMethod: 'UnsubscribeFromPortfolio',
    },
    portfolioId,
    refresh
  )

  async function load(id: number) {
    if (portfolioId.value !== id) {
      await unsubscribe()
      reset()
      portfolioId.value = id
    }

    await subscribe(id)
    await refresh()
  }

  async function refresh() {
    if (!portfolioId.value) return

    isLoading.value = true
    error.value = null

    try {
      const [portfolioResult, positionsResult, exposureResult, instrumentTypesResult] =
        await Promise.all([
          getPortfolioDetail(portfolioId.value),
          getPortfolioPositions(portfolioId.value),
          getPortfolioExposure(portfolioId.value),
          getPortfolioInstrumentTypes(portfolioId.value),
        ])

      portfolio.value = portfolioResult
      positions.value = positionsResult
      exposure.value = exposureResult
      instrumentTypes.value = instrumentTypesResult
      lastUpdated.value = new Date()
    } catch (e) {
      error.value = 'Failed to load portfolio data'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    portfolioId.value = null
    portfolio.value = null
    positions.value = null
    exposure.value = null
    instrumentTypes.value = null
    lastUpdated.value = null
    error.value = null
  }

  return {
    // identity
    portfolioId,

    // raw state
    portfolio,
    positions,
    exposure,
    instrumentTypes,

    // derived state
    openPositionsList,

    // lifecycle
    isLoading,
    lastUpdated,
    error,

    // actions
    load,
  }
})
