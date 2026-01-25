import { defineStore } from "pinia";
import { ref } from "vue";
import { getPortfolioList } from "@/api/portfoliosApi";
import type { PortfolioSummaryDto } from "@/api/dtos/PortfolioSummaryDto";

export const useNavigationStore = defineStore("navigation", () => {
  const portfolios = ref<PortfolioSummaryDto[]>([]);
  const isLoading = ref(false);
  const lastUpdated = ref<Date | null>(null);
  const error = ref<string | null>(null);

  async function loadPortfolios() {
    isLoading.value = true;
    error.value = null;
    try {
      portfolios.value = await getPortfolioList();
      lastUpdated.value = new Date();
    } catch (e: any) {
      error.value = "Failed to load portfolios";
      console.error("Failed to load portfolios:", e);
      portfolios.value = [];
    } finally {
      isLoading.value = false;
    }
  }

  async function refresh() {
    await loadPortfolios();
  }

  return {
    portfolios,
    isLoading,
    lastUpdated,
    error,
    loadPortfolios,
    refresh
  };
});
