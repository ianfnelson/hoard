import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { getPortfolioDetail, getPortfolioPositions } from "@/api/portfoliosApi";
import type { PortfolioDetailDto } from "@/api/dtos/PortfolioDetailDto.ts";
import type {PortfolioPositionsDto} from "@/api/dtos/PortfolioPositionsDto.ts";

export const usePortfolioOverviewStore = defineStore("portfolioOverview", () => {
  const portfolioId = ref<number | null>(null);

  const portfolio = ref<PortfolioDetailDto | null>(null);
  const positions = ref<PortfolioPositionsDto | null>(null);

  const isLoading = ref(false)
  const lastUpdated = ref<Date | null>(null)
  const error = ref<string | null>(null)

  const openPositions = computed(() => {
    if (!positions.value) {
      return [];
    }

    return positions.value.positions.filter(
      p => p.closeDate === null
    );
  });

  async function load(id: number) {
    if (portfolioId.value !== id) {
      reset()
      portfolioId.value = id
    }

    await refresh()
  }

  async function refresh() {
    if (!portfolioId.value) return

    isLoading.value = true
    error.value = null

    try {
      const [portfolioResult, positionsResult] = await Promise.all([
        getPortfolioDetail(portfolioId.value),
        getPortfolioPositions(portfolioId.value)
      ])

      portfolio.value = portfolioResult
      positions.value = positionsResult
      lastUpdated.value = new Date()
    } catch (e: any) {
      error.value = "Failed to load portfolio overview";
      throw e
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    portfolioId.value = null;
    portfolio.value = null;
    positions.value = null;
    lastUpdated.value = null;
    error.value = null;
  }

  return {
    // identity
    portfolioId,

    // raw state
    portfolio,

    // derived state
    openPositions,

    // lifecycle
    isLoading,
    lastUpdated,
    error,

    // actions
    load
  };
});
